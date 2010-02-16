/*
 * File for the fun ends
 * Death or a total win
 *
 * @(#)rip.c	4.57 (Berkeley) 02/05/99
 *
 * Rogue: Exploring the Dungeons of Doom
 * Copyright (C) 1980-1983, 1985, 1999 Michael Toy, Ken Arnold and Glenn Wichman
 * All rights reserved.
 *
 * See the file LICENSE.TXT for full copyright and licensing information.
 */

using System;

partial class Rogue
{
    // Do something really fun when he dies
    void death(char monst)
    {
        Console.Clear();
        Console.WriteLine("Killed!");
        my_exit(0);
    }

    // Code for a winner
    void total_winner()
    {
        Console.Clear();
        Console.WriteLine("You win!");
        my_exit(0);
    }
}
