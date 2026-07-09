from pathlib import Path
import re

root = Path(r'd:\users\Anonymous\Documents\C Sharp\VirtuoPhoneGamePadWin')
files = list(root.rglob('Models/**/*.cs'))

print('Reindenting', len(files), 'files')

for path in files:
    text = path.read_text(encoding='utf-8')
    lines = text.splitlines()
    indent = 0
    out_lines = []

    for line in lines:
        stripped = line.strip()
        if stripped == '':
            out_lines.append('')
            continue

        # handle lines that should be dedented before output
        dedent_before = False
        if stripped.startswith('}') or stripped.startswith('} '):
            indent = max(indent - 1, 0)
            dedent_before = True

        # keep namespace file-scoped style line alone
        if stripped.startswith('namespace ') and stripped.endswith(';'):
            out_lines.append(stripped)
            continue

        # indent line
        out_lines.append(' ' * (4 * indent) + stripped)

        # count braces outside strings
        line_no_strings = re.sub(r'"(\\.|[^"\\])*"', '""', stripped)
        brace_open = line_no_strings.count('{')
        brace_close = line_no_strings.count('}')

        # adjust indent after line
        if stripped.endswith('{') and not stripped.startswith('case '):
            indent += 1
        elif brace_open > brace_close:
            indent += brace_open - brace_close
        elif brace_close > brace_open and not dedent_before:
            indent = max(indent - (brace_close - brace_open), 0)

    path.write_text('\n'.join(out_lines) + '\n', encoding='utf-8')
    print('Reindented', path)
print('Done')
