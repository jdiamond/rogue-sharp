/*
 * Defines for things used in mach_dep.c
 *
 * @(#)extern.h	4.35 (Berkeley) 02/05/99
 *
 * Rogue: Exploring the Dungeons of Doom
 * Copyright (C) 1980-1983, 1985, 1999 Michael Toy, Ken Arnold and Glenn Wichman
 * All rights reserved.
 *
 * See the file LICENSE.TXT for full copyright and licensing information.
 */

partial class Rogue
{
    const int MAXLINES = 32; /* maximum number of screen lines used */
    const int MAXCOLS = 80; /* maximum number of screen columns used */

    int RN()
    {
        return (((seed = seed*11109 + 13849) >> 16) & 0xffff);
    }

    static char CTRL(char c)
    {
        return (char)(c & 0x1F);
    }
}