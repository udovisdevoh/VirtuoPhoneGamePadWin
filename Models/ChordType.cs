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
    pentatonic_minor,
    pentatonic_major,
    pentatonic_minor_blues,
    pentatonic_major_blues,
    pentatonic_mixolydian,
    pentatonic_minor_flat5,
    pentatonic_suspended,
    pentatonic_scottish,
#warning ajouter ces types d'accord / gamme
    sus4sharp,
    sus2flat,
    ionian,
    dorian,
    phrygian,
    lydian,
    mixolydian,
    aeolian,
    locrian,
    harmonic_minor,
    phrygian_dominant,
    melodic_minor,
    mixolydian_b6,
    lydian_dominant,
    double_harmonic,
    double_harmonic_major,
    dorian_sharp_4
}
