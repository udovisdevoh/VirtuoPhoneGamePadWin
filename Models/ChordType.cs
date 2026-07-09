using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace VirtuoPhone.Models;

public enum ChordType
{
    maj, maj7, maj9, maj13,
    m, m6, m7, m9,
    m11, m13, mma, aug, dim, dim7,
    aug9, add9, sus2, sus4,
    five, six, sixSlashNine, seven,
    sevenSusFour, sevenBFive, sevenBNine, nine,
    nineSusFour, eleven, thirteen, ff_maj, ff_min,
#warning todo add these type of "chords" -> (in fact they are scales) to the rest of the code
    pentatonic_minor,
    pentatonic_major,
    pentatonic_minor_blues
}
