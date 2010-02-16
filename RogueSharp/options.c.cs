/*
 * This file has all the code for the option command.  I would rather
 * this command were not necessary, but it is the only way to keep the
 * wolves off of my back.
 *
 * @(#)options.c	4.24 (Berkeley) 05/10/83
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
    int MAXINP = 50;	/* max string to read from terminal or environment */

    int get_str(out string vopt, IntPtr win)
    {
        vopt = "";
        string opt = vopt;
        int sp;
        int oy, ox;
        int i;
        int c;
        char[] buf = new char[MAXINP];

        getyx(win, out oy, out ox);
        wrefresh(win);
        /*
         * loop reading in the string, and put it in a temporary buffer
         */
        for (sp = 0; (c = readchar()) != '\n' && c != '\r' && c != ESCAPE;
            wclrtoeol(win), wrefresh(win))
        {
            if (c == -1)
                continue;
            else if (c == erasechar())	/* process erase character */
            {
                if (sp > 0)
                {
                    sp--;
                    for (i = unctrl(buf[sp]).Length; i > 0; i--)
                        waddch(win, '\b');
                }
                continue;
            }
            else if (c == killchar())	/* process kill character */
            {
                sp = 0;
                wmove(win, oy, ox);
                continue;
            }
            else if (sp == 0)
            {
                if (c == '-' && win != stdscr)
                    break;
                //else if (c == '~')
                //{
                //strcpy(buf, home);
                //waddstr(win, home);
                //sp += strlen(home);
                //continue;
                //}
            }
            if (sp >= MAXINP || char.IsControl((char)c))
                Console.Beep();
            else
            {
                buf[sp] = (char)c;
                waddstr(win, unctrl((char)c));
                sp++;
            }
        }
        if (sp > 0)	/* only change option if something has been typed */
        {
            opt = new string(buf);
            vopt = opt;
        }
        mvwprintw(win, oy, ox, "{0}\n", opt);
        wrefresh(win);
        if (win == stdscr)
            mpos += sp;
        if (c == '-')
            return MINUS;
        else if (c == ESCAPE)
            return QUIT;
        else
            return NORM;
    }
}
