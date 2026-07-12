using VirtuoPhone.Input;
using VirtuoPhone.Models;

namespace VirtuoPhone.Config;

/// <summary>One grid cell: an absolute chord (root note type 0..11 + chord/scale type).</summary>
public sealed class Cell
{
    public int Root { get; set; }              // note type 0..11 (C..B)
    public ChordType Type { get; set; } = ChordType.maj;

    public Cell() { }
    public Cell(int root, ChordType type) { Root = root; Type = type; }
    public Cell Clone() => new(Root, Type);
    public override string ToString() => new Note(Root % 12, 3).GetName() + " " + Type;
}

/// <summary>
/// A playable preset: the 3×3 inner cells (all nine <see cref="Direction"/>s) plus four outer "dash" cells
/// (the cardinals), each an explicit chord. The <see cref="Direction.Neutral"/> inner cell is the home center;
/// modulation transposes every cell by the interval between the aimed cell and the center.
/// </summary>
public sealed class Preset
{
    public string Name { get; set; } = "Untitled";
    public Dictionary<Direction, Cell> Inner { get; set; } = new();
    public Dictionary<Direction, Cell> Outer { get; set; } = new();

    public static readonly Direction[] AllInner =
    {
        Direction.UpLeft, Direction.Up, Direction.UpRight,
        Direction.Left,   Direction.Neutral, Direction.Right,
        Direction.DownLeft, Direction.Down, Direction.DownRight,
    };
    public static readonly Direction[] Cardinals = { Direction.Up, Direction.Right, Direction.Down, Direction.Left };

    /// <summary>The default "Chromatic Spiral" (CLAUDE.md Example 2), center E major, dim7 outer cells.</summary>
    public static Preset Default() => new()
    {
        Name = "Chromatic Spiral",
        Inner = new()
        {
            [Direction.UpLeft]    = new(Note.C,      ChordType.maj),
            [Direction.Up]        = new(Note.D,      ChordType.maj),
            [Direction.UpRight]   = new(Note.F,      ChordType.maj),
            [Direction.Left]      = new(Note.B,      ChordType.maj),
            [Direction.Neutral]   = new(Note.E,      ChordType.maj),
            [Direction.Right]     = new(Note.FSharp, ChordType.maj),
            [Direction.DownLeft]  = new(Note.ASharp, ChordType.maj),
            [Direction.Down]      = new(Note.A,      ChordType.maj),
            [Direction.DownRight] = new(Note.G,      ChordType.maj),
        },
        Outer = new()
        {
            [Direction.Up]    = new(Note.C,      ChordType.dim7),
            [Direction.Right] = new(Note.CSharp, ChordType.dim7),
            [Direction.Down]  = new(Note.B,      ChordType.dim7),
            [Direction.Left]  = new(Note.B,      ChordType.dim7),
        },
    };

    public int CenterRoot => Inner.TryGetValue(Direction.Neutral, out var c) ? c.Root : Note.E;

    /// <summary>Fill any missing cells so a hand-edited / partial preset is always playable.</summary>
    public void Normalize()
    {
        var def = Default();
        foreach (var d in AllInner)
            if (!Inner.ContainsKey(d)) Inner[d] = def.Inner[d].Clone();
        foreach (var d in Cardinals)
            if (!Outer.ContainsKey(d)) Outer[d] = def.Outer[d].Clone();
    }

    public Preset Clone() => new()
    {
        Name = Name,
        Inner = Inner.ToDictionary(kv => kv.Key, kv => kv.Value.Clone()),
        Outer = Outer.ToDictionary(kv => kv.Key, kv => kv.Value.Clone()),
    };
}
