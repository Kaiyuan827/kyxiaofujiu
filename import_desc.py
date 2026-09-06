# -*- coding: utf-8 -*-
"""
名称：描述规范化表 -> localization/zhs JSON 批量回写脚本
作用：把「描述规范化表.xlsx」中 卡牌 / 能力 / 遗物 / 术语 四个 sheet 的
      「修改后的(卡牌)描述」列回写到 mod 的 localization/zhs/*.json。

设计原则（与 说明备忘 约定一致）：
  1. 只回写「修改后的描述」列，绝不全表覆盖。
  2. 只改动 Excel 里出现的 key，其它 key（selectPrompt / smartDescription /
     banter / progressDesc / flavor / title 等）原样保留。
  3. 回写前校验 JSON 可解析、key 存在；key 缺失时默认 SKIP 并报告（--create-missing 才会新建）。
  4. 写文件前把原文件备份到 backup/ 下，并输出每文件的 unified diff。

用法（在 mod 根目录 / 本项目目录均可，脚本会自动定位路径）：
    python import_desc.py                       # 预览（dry-run，只打印将改动项，不写文件）
    python import_desc.py --apply               # 真正写回
    python import_desc.py --apply --no-backup   # 写回但不备份
    python import_desc.py --create-missing      # 允许为缺失的 key 新建条目（默认关闭）
"""

from __future__ import annotations

import argparse
import copy
import difflib
import json
import os
import re
import shutil
import sys
import time

import openpyxl


# --------------------------------------------------------------------------- #
# 目录 / 路径
# --------------------------------------------------------------------------- #
SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
EXCEL = os.path.join(SCRIPT_DIR, "描述规范化表.xlsx")
LOC_DIR = os.path.join(SCRIPT_DIR, "kyxiaofujiu", "localization", "zhs")
BACKUP_DIR = os.path.join(SCRIPT_DIR, "backup")


# --------------------------------------------------------------------------- #
# 术语 sheet 的特殊映射（不能靠「标题->id」反查，因为机制词条没有 .title）
# --------------------------------------------------------------------------- #
TERM_TARGETS = {
    "夫黑": ("cards.json", "FUZU_BLACK.description"),
    "夫白": ("cards.json", "FUZU_WHITE.description"),
    "夫黄": ("cards.json", "FUZU_YELLOW.description"),
    "转化": ("cards.json", "CONVERT_MECHANICS_DESC"),
    "夫黑打出": ("cards.json", "FUHEI_PLAY_MECHANICS_DESC"),
    "征婚机制": ("cards.json", "MARRIAGE_MECHANICS_DESC"),
    "工资": ("cards.json", "SALARY_MECHANICS_DESC"),
    "打工状态": ("cards.json", "DAGONG_MECHANICS_DESC"),
    "完美格挡": ("cards.json", "PERFECT_BLOCK_MECHANICS_DESC"),
}


# --------------------------------------------------------------------------- #
# Excel 解析
# --------------------------------------------------------------------------- #
def read_sheet_rows(ws):
    """按表头读取每一行，返回 [{header: value}]。"""
    headers = [ws.cell(1, c).value for c in range(1, ws.max_column + 1)]
    rows = []
    for r in range(2, ws.max_row + 1):
        first = ws.cell(r, 1).value
        if first is None:
            continue
        rec = {}
        for c in range(1, ws.max_column + 1):
            rec[headers[c - 1]] = ws.cell(r, c).value
        rows.append(rec)
    return rows


def load_excel(path):
    return openpyxl.load_workbook(path, data_only=True)


def rows_for(wb, sheet):
    return read_sheet_rows(wb[sheet])


# --------------------------------------------------------------------------- #
# 现有 localization 加载 & 反查表
# --------------------------------------------------------------------------- #
def load_loc_tables(loc_dir):
    tables = {}
    for fn in os.listdir(loc_dir):
        if not fn.endswith(".json"):
            continue
        with open(os.path.join(loc_dir, fn), encoding="utf-8") as f:
            tables[fn] = json.load(f)
    return tables


def build_title_map(tables, fname):
    m = {}
    for k, v in tables.get(fname, {}).items():
        if k.endswith(".title"):
            title = str(v).strip()
            m.setdefault(title, []).append(k[: -len(".title")])
    return m


# --------------------------------------------------------------------------- #
# 生成回写操作
# --------------------------------------------------------------------------- #
def build_operations(wb, tables):
    ops = []
    missing = []

    card_titles = build_title_map(tables, "cards.json")
    power_titles = build_title_map(tables, "powers.json")
    relic_titles = build_title_map(tables, "relics.json")
    seen = set()

    def add(op_file, key, new_value, source, remark=None):
        tbl = tables.get(op_file, {})
        if (op_file, key) in seen:
            return
        seen.add((op_file, key))
        if key not in tbl:
            missing.append((op_file, key, source))
            return
        ops.append(
            {
                "file": op_file,
                "key": key,
                "new_value": new_value,
                "source": source,
                "remark": remark,
            }
        )

    # ---- 卡牌 sheet -> cards.json .description ----
    for rec in rows_for(wb, "卡牌"):
        title = str(rec.get("标题") or "").strip()
        new_val = rec.get("修改后的卡牌描述")
        if not title or new_val in (None, ""):
            continue
        ids = card_titles.get(title, [])
        if not ids:
            missing.append(("cards.json", f"{title}-><id>.description", f"卡牌[{title}]"))
            continue
        for cid in ids:
            add("cards.json", f"{cid}.description", new_val, f"卡牌[{title}]", rec.get("备注"))

    # ---- 能力 sheet -> powers.json .description / .smartDescription ----
    for rec in rows_for(wb, "能力"):
        title = str(rec.get("标题") or "").strip()
        new_val = rec.get("修改后的描述")
        indep = str(rec.get("是否独立description") or "").strip()
        if not title or new_val in (None, ""):
            continue
        ids = power_titles.get(title, [])
        if not ids:
            missing.append(("powers.json", f"{title}-><_POWER>", f"能力[{title}]"))
            continue
        for pid in ids:
            if indep == "有":
                add("powers.json", f"{pid}.description", new_val, f"能力[{title}]", rec.get("备注"))
                # 若该能力同时存在 smartDescription，战斗内悬浮优先显示它，这里也一并回写，
                # 否则改动只对“没有 smartDescription 时/原型实例”生效，战斗内看不到。
                if f"{pid}.smartDescription" in tables.get("powers.json", {}):
                    add("powers.json", f"{pid}.smartDescription", new_val, f"能力[{title}]", rec.get("备注"))
            else:
                add("powers.json", f"{pid}.smartDescription", new_val, f"能力[{title}]", rec.get("备注"))

    # ---- 遗物 sheet -> relics.json .description ----
    for rec in rows_for(wb, "遗物"):
        title = str(rec.get("标题") or "").strip()
        new_val = rec.get("修改后的描述")
        if not title or new_val in (None, ""):
            continue
        ids = relic_titles.get(title, [])
        if not ids:
            missing.append(("relics.json", f"{title}-><id>.description", f"遗物[{title}]"))
            continue
        for rid in ids:
            add("relics.json", f"{rid}.description", new_val, f"遗物[{title}]", rec.get("备注"))

    # ---- 术语 sheet -> cards.json（机制词条 / 三形态卡牌，硬映射）----
    for rec in rows_for(wb, "术语"):
        title = str(rec.get("标题") or "").strip()
        new_val = rec.get("修改后的描述")
        if not title or new_val in (None, ""):
            continue
        tgt = TERM_TARGETS.get(title)
        if tgt is None:
            missing.append(("card.json 未知术语", f"{title}", f"术语[{title}]"))
            continue
        op_file, key = tgt
        add(op_file, key, new_val, f"术语[{title}]", rec.get("备注"))

    return ops, missing


# --------------------------------------------------------------------------- #
# 手术式写回：只替换目标 key 所在物理行，其余内容（含空行）原样保留
# --------------------------------------------------------------------------- #
def _json_string_end(text, start):
    """start 指向值字符串的开头引号，返回闭合引号下标+1；找不到返回 -1。"""
    i = start + 1
    n = len(text)
    while i < n:
        c = text[i]
        if c == "\\":
            i += 2
            continue
        if c == '"':
            return i + 1
        i += 1
    return -1


def replace_json_value(text, key, new_value):
    """只替换 `"key": "..."` 里的字符串值 token，其余字符（含同行其它条目）原样保留。"""
    encoded = json.dumps(new_value, ensure_ascii=False)
    pattern = re.compile(
        r'(?<=[\s{,])"' + re.escape(key) + r'"\s*:\s*"'
    )
    m = pattern.search(text)
    if not m:
        return text, False
    val_start = m.end() - 1  # 值开头引号
    val_end = _json_string_end(text, val_start)
    if val_end == -1:
        return text, False
    return text[:val_start] + encoded + text[val_end:], True


def apply_ops_to_file(fname, ops, tables, create_missing):
    path = os.path.join(LOC_DIR, fname)
    with open(path, encoding="utf-8") as f:
        text = f.read()
    desired = copy.deepcopy(tables[fname])
    changed = False
    for op in ops:
        if op["file"] != fname:
            continue
        key = op["key"]
        if key not in desired and not create_missing:
            continue
        desired[key] = op["new_value"]
        text, ok = replace_json_value(text, key, op["new_value"])
        if ok:
            changed = True
        else:
            print(f"  [WARN] 未能行级替换 {fname}:{key}，将用 JSON 全量重写该文件")
    if not changed:
        return text, True
    try:
        parsed = json.loads(text)
    except Exception:  # noqa: BLE001
        text = json.dumps(desired, ensure_ascii=False, indent=2)
        parsed = desired
    if parsed != desired:
        text = json.dumps(desired, ensure_ascii=False, indent=2)
        parsed = desired
    return text, False


def unified_diff(old, new, fname):
    old_lines = old.splitlines(keepends=True)
    new_lines = new.splitlines(keepends=True)
    return "".join(
        difflib.unified_diff(
            old_lines,
            new_lines,
            fromfile="a/" + fname,
            tofile="b/" + fname,
            lineterm="\n",
        )
    )


def main():
    ap = argparse.ArgumentParser(description="描述规范化表 -> zhs JSON 回写")
    ap.add_argument("--apply", action="store_true", help="真正写回（默认 dry-run）")
    ap.add_argument("--no-backup", action="store_true", help="写回时不生成备份")
    ap.add_argument("--create-missing", action="store_true", help="允许为缺失 key 新建条目")
    args = ap.parse_args()

    if not os.path.exists(EXCEL):
        sys.exit(f"找不到 Excel：{EXCEL}")
    if not os.path.isdir(LOC_DIR):
        sys.exit(f"找不到 localization 目录：{LOC_DIR}")

    wb = load_excel(EXCEL)
    tables = load_loc_tables(LOC_DIR)
    ops, missing = build_operations(wb, tables)

    if not ops:
        print("没有需要回写的字段。")

    by_file = {}
    for op in ops:
        by_file.setdefault(op["file"], []).append(op)

    print(f"共 {len(ops)} 个字段待回写，涉及文件：{sorted(by_file)}")
    if missing:
        print("\n[missing] 以下 key 在当前 JSON 中不存在（默认跳过，可用 --create-missing 新建）：")
        for fn, key, src in missing:
            print(f"   {fn} : {key}   <- {src}")

    diff_buf = []
    total_written = 0
    for fname in sorted(by_file):
        with open(os.path.join(LOC_DIR, fname), encoding="utf-8") as f:
            old_text = f.read()
        new_text, _ = apply_ops_to_file(fname, by_file[fname], tables, args.create_missing)
        if new_text == old_text:
            print(f"[skip] {fname}：无变化")
            continue
        d = unified_diff(old_text, new_text, fname)
        if d:
            diff_buf.append(d)
        print(f"[{'写入' if args.apply else '预览'}] {fname}：{len(by_file[fname])} 个字段")
        if args.apply and not args.no_backup:
            os.makedirs(BACKUP_DIR, exist_ok=True)
            stamp = time.strftime("%Y%m%d_%H%M%S")
            bak_dst = os.path.join(BACKUP_DIR, f"pre_import_{stamp}")
            os.makedirs(bak_dst, exist_ok=True)
            shutil.copy2(
                os.path.join(LOC_DIR, fname),
                os.path.join(bak_dst, fname),
            )
            with open(os.path.join(LOC_DIR, fname), "w", encoding="utf-8", newline="") as f:
                f.write(new_text)
            total_written += 1

    if diff_buf:
        report = os.path.join(SCRIPT_DIR, "import_diff.txt")
        with open(report, "w", encoding="utf-8") as f:
            f.write("\n".join(diff_buf))
        print(f"\n差异已写入：{report}")

    print("\n完成。" + ("写回文件数：" + str(total_written) if args.apply else "（dry-run，未写文件）"))


if __name__ == "__main__":
    main()
