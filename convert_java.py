from pathlib import Path
import re

root = Path(r'd:\users\Anonymous\Documents\C Sharp\VirtuoPhoneGamePadWin')
java_files = []
for p in root.rglob('*.cs'):
    text = p.read_text(encoding='utf-8')
    if 'package com.virtuophone' in text:
        java_files.append(p)

print('Found', len(java_files), 'files')
for p in java_files:
    text = p.read_text(encoding='utf-8')
    text = text.replace('package com.virtuophone.models.instruments;', 'namespace VirtuoPhone.Models;')
    text = text.replace('package com.virtuophone.models;', 'namespace VirtuoPhone.Models;')
    text = re.sub(r'^import .*;$', '', text, flags=re.MULTILINE)
    text = re.sub(r'@Override\s*\n', '', text)
    text = text.replace('public interface StringSerializable', 'public interface IStringSerializable')
    text = re.sub(r'implements Iterable<([^>]+)>, IStringSerializable', r': IEnumerable<\1>, IStringSerializable', text)
    text = re.sub(r'implements Iterable<([^>]+)>', r': IEnumerable<\1>', text)
    text = text.replace('extends Instrument', ': Instrument')
    text = text.replace('super(context);', 'base(context);')
    text = re.sub(r'\bString\b', 'string', text)
    text = re.sub(r'\bboolean\b', 'bool', text)
    text = re.sub(r'\bArrayList<', 'List<', text)
    text = re.sub(r'\bVector<', 'List<', text)
    text = re.sub(r'\bIterator<', 'IEnumerator<', text)
    text = text.replace('.iterator()', '.GetEnumerator()')
    text = text.replace('.size()', '.Count')
    text = text.replace('.length()', '.Length')
    text = text.replace('.trim()', '.Trim()')
    text = text.replace('.split("', '.Split("')
    text = re.sub(r'\.split\(("[^"]*")\)', r'.Split(\1)', text)
    text = text.replace('Math.abs', 'Math.Abs')
    text = text.replace('Math.pow', 'Math.Pow')
    text = re.sub(r'([a-zA-Z0-9_]+)\.get\(([^)]+)\)', r'\1[\2]', text)
    text = re.sub(r'([a-zA-Z0-9_]+)\.set\(([^,]+),\s*([^\)]+)\)', r'\1[\2] = \3', text)
    text = re.sub(r'\bnew List<([^>]+)>\(\);', r'new List<\1>();', text)
    text = re.sub(r'\bnew HashSet<([^>]+)>\(\);', r'new HashSet<\1>();', text)
    
    # Add using declarations if missing
    lines = text.splitlines()
    if any(line.startswith('namespace ') for line in lines):
        first_ns = next(i for i, line in enumerate(lines) if line.startswith('namespace '))
        prefix = [line for line in lines[:first_ns] if line.strip() != '']
        rest = lines[first_ns:]
        existing = {line for line in prefix if line.startswith('using ')}
        usings = [
            'using System;',
            'using System.Collections;',
            'using System.Collections.Generic;',
            'using System.Text;',
            'using System.Drawing;'
        ]
        insert = [u for u in usings if u not in existing]
        text = '\n'.join(insert + [''] + prefix + rest)

    # Append explicit interface implementation if needed
    if 'IEnumerable<' in text and 'IEnumerator IEnumerable.GetEnumerator()' not in text:
        if text.rstrip().endswith('}'):
            text = text.rstrip()
            text = text[:-1].rstrip() + '\n    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();\n}'

    p.write_text(text, encoding='utf-8')
    print('Converted', p)
print('Done')
