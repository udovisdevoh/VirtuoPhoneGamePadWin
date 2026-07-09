from pathlib import Path
import re
root = Path(r'd:\users\Anonymous\Documents\C Sharp\VirtuoPhoneGamePadWin')
files = [p for p in root.rglob('*.cs') if p.is_file()]
java_files = []
for p in files:
    text = p.read_text(encoding='utf-8')
    if 'namespace VirtuoPhone.Models' in text or 'namespace VirtuoPhone.Models.Instruments' in text:
        if re.search(r'\bpublic (class|interface|enum) ', text):
            if 'IStringSerializable' in text or 'StringSerializable' in text or 'Context' in text or 'SoundPool' in text:
                java_files.append(p)
print('Fixing', len(java_files), 'files')
for p in java_files:
    text = p.read_text(encoding='utf-8')
    # rename interface and references
    text = text.replace('public interface StringSerializable', 'public interface IStringSerializable')
    text = re.sub(r'\bStringSerializable\b', 'IStringSerializable', text)
    # convert Java collections and methods
    text = re.sub(r'\b(new )?ArrayList<', r'new List<', text)
    text = re.sub(r'\b(new )?Vector<', r'new List<', text)
    text = re.sub(r'\.add\(', '.Add(', text)
    text = re.sub(r'\.remove\(', '.Remove(', text)
    text = re.sub(r'\.size\(', '.Count(', text)
    text = re.sub(r'\.length\b', '.Length', text)
    text = re.sub(r'\.trim\(', '.Trim(', text)
    text = re.sub(r'\.append\(', '.Append(', text)
    text = re.sub(r'\.replace\(', '.Replace(', text)
    text = re.sub(r'\.toString\(', '.ToString(', text)
    text = re.sub(r'\.equals\(', '.Equals(', text)
    text = re.sub(r'\.iterator\(', '.GetEnumerator(', text)
    text = re.sub(r'\bStringBuilder\b', 'StringBuilder', text)
    text = re.sub(r'\bboolean\b', 'bool', text)
    text = re.sub(r'\bimport .*;$', '', text, flags=re.MULTILINE)
    text = text.replace('Math.pow', 'Math.Pow')
    text = text.replace('Math.abs', 'Math.Abs')
    # Java style for-each loops
    text = re.sub(r'for \(([^\s]+) ([^:]+): ([^\)]+)\)', r'foreach (\1 \2 in \3)', text)
    # Java generics types changed in interface declarations etc.
    text = text.replace(': IEnumerable<', ': IEnumerable<')
    # Replace Java array indexing patterns .get(index) and .set(index, value)
    text = re.sub(r'([a-zA-Z_][a-zA-Z0-9_]*)\.get\(([^)]+)\)', r'\1[\2]', text)
    text = re.sub(r'([a-zA-Z_][a-zA-Z0-9_]*)\.set\(([^,]+),\s*([^\)]+)\)', r'\1[\2] = \3', text)
    # fix method naming/returns
    text = re.sub(r'public (?:int|bool|float|string|void) get([A-Z])', lambda m: 'public ' + m.group(0)[7:].replace('get', 'Get', 1), text)
    # Specific Java syntax not valid in C#
    text = text.replace('public class Chord : IEnumerable<Note>, IStringSerializable', 'public class Chord : IEnumerable<Note>, IStringSerializable')
    text = text.replace('public class GuitarPreset : IEnumerable<Chord>, IStringSerializable', 'public class GuitarPreset : IEnumerable<Chord>, IStringSerializable')
    # fix array methods
    text = re.sub(r'\bif \(([^)]+)\.Length > 0\)', r'if (\1.Length > 0)', text)
    text = re.sub(r'while \(([^)]+)\.Length >= 0\)', r'while (\1.Length >= 0)', text)
    p.write_text(text, encoding='utf-8')
    print('Fixed', p)
print('Done')
