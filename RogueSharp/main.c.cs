/*
 * Rogue: Exploring the Dungeons of Doom
 * Copyright (C) 1980-1983, 1985, 1999 Michael Toy, Ken Arnold and Glenn Wichman
 * All rights reserved.
 *
 * See the file LICENSE.TXT for full copyright and licensing information.
 *
 * @(#)main.c	4.22 (Berkeley) 02/05/99
 */

using System;

partial class Rogue
{
    void main()
    {
        initscr();
        hw = newwin(LINES, COLS, 0, 0);

        init_probs();			/* Set up prob tables for objects */
        init_player();			/* Set up initial player stats */
        init_names();			/* Set up names of scrolls */
        init_colors();			/* Set up colors of potions */
        init_stones();			/* Set up stone settings of rings */
        init_materials();			/* Set up materials of wands */

        new_level();

        start_daemon(runners, 0, AFTER);
        start_daemon(doctor, 0, AFTER);
        fuse(swander, 0, WANDERTIME, AFTER);
        start_daemon(stomach, 0, AFTER);

        playit();
    }

    // Exit the program abnormally.
    void endit(int sig)
    {
        fatal("Okay, bye bye!\n");
    }

    // Exit the program, printing a message.
    void fatal(string s)
    {
        Console.Clear();
        Console.WriteLine(s);
        Environment.Exit(0);
    }

    // Pick a very random number.
    int rnd(int range)
    {
        return rnd2(range, null);
    }

    int rnd2(int range, string key)
    {
        return this.rng.Next(key, range);
    }

    // Roll a number of dice
    int roll(int number, int sides)
    {
        int dtotal = 0;

        while (number-- > 0)
            dtotal += rnd(sides) + 1;
        return dtotal;
    }

    // The main loop of the program.  Loop until the game is over,
    // refreshing things and looking at the proper times.
    void playit()
    {
        oldpos = new coord();
        oldpos.CopyFrom(hero);
        oldrp = roomin(hero);
        while (playing)
            command();			/* Command execution */
        endit(0);
    }

    // Have player make certain, then exit.
    void quit(int sig)
    {
        int oy, ox;

        /*
         * Reset the signal in case we got here via an interrupt
         */
        if (!q_comm)
            mpos = 0;
        getyx(curscr, out oy, out ox);
        msg("really quit?");
        if (readchar() == 'y')
        {
            //signal(SIGINT, leave);
            clear();
            mvprintw(LINES - 2, 0, "You quit with {0} gold pieces", purse);
            move(LINES - 1, 0);
            refresh();
            //@jd score(purse, 1, 0);
            my_exit(0);
        }
        else
        {
            move(0, 0);
            clrtoeol();
            status();
            move(oy, ox);
            refresh();
            mpos = 0;
            count = 0;
            to_death = false;
        }
    }

    // Leave the process properly
    void my_exit(int st)
    {
        //resetltchars();
        Environment.Exit(st);
    }
}