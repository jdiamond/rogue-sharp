/*
 * All sorts of miscellaneous routines
 *
 * @(#)misc.c	4.66 (Berkeley) 08/06/83
 *
 * Rogue: Exploring the Dungeons of Doom
 * Copyright (C) 1980-1983, 1985, 1999 Michael Toy, Ken Arnold and Glenn Wichman
 * All rights reserved.
 *
 * See the file LICENSE.TXT for full copyright and licensing information.
 */

partial class Rogue
{
    // A quick glance all around the player
    void look(bool wakeup)
    {
        int x, y;
        int ch;
        THING tp;
        PLACE pp;
        room rp;
        int ey, ex;
        int passcount;
        char pch;
        int pfl;
        int fp;
        int sy, sx, sumhero = 0, diffhero = 0;
        passcount = 0;
        rp = proom;
        if (!ce(oldpos, hero))
        {
            erase_lamp(oldpos, oldrp);
            oldpos.CopyFrom(hero);
            oldrp = rp;
        }
        ey = hero.y + 1;
        ex = hero.x + 1;
        sx = hero.x - 1;
        sy = hero.y - 1;
        if (door_stop && !firstmove && running)
        {
            sumhero = hero.y + hero.x;
            diffhero = hero.y - hero.x;
        }
        pp = INDEX(hero.y, hero.x);
        pch = pp.p_ch;
        pfl = pp.p_flags;

        for (y = sy; y <= ey; y++)
            if (y > 0 && y < NUMLINES - 1) for (x = sx; x <= ex; x++)
            {
                if (x < 0 || x >= NUMCOLS)
                    continue;
                if (!on(player, ISBLIND))
                {
                    if (y == hero.y && x == hero.x)
                        continue;
                }

                pp = INDEX(y, x);
                ch = pp.p_ch;
                if (ch == ' ')		/* nothing need be done with a ' ' */
                    continue;
                fp = pp.p_flags;
                if (pch != DOOR && ch != DOOR)
                    if ((pfl & F_PASS) != (fp & F_PASS))
                        continue;
                if ((((fp & F_PASS) != 0) || ch == DOOR) && 
                     (((pfl & F_PASS) != 0) || pch == DOOR))
                {
                    if (hero.x != x && hero.y != y &&
                            !step_ok(chat(y, hero.x)) && !step_ok(chat(hero.y, x)))
                        continue;
                }

                if ((tp = pp.p_monst) == null)
                    ch = trip_ch(y, x, ch);
                else
                    if (on(player, SEEMONST) && on(tp, ISINVIS))
                    {
                        if (door_stop && !firstmove)
                            running = false;
                        continue;
                    }
                    else
                    {
                        if (wakeup)
                            wake_monster(y, x);
                        if (see_monst(tp))
                        {
                            if (on(player, ISHALU))
                                ch = rnd(26) + 'A';
                            else
                                ch = tp.t_disguise;
                        }
                    }
                    if (on(player, ISBLIND) && (y != hero.y || x != hero.x))
                        continue;
                    move(y, x);

                    if (((proom.r_flags & ISDARK) != 0) && !see_floor && ch == FLOOR)
                        ch = ' ';

                    if (tp != null || ch != CCHAR( inch() ))
                        addch(ch);

                    if (door_stop && !firstmove && running)
                    {
                        switch (runch)
                        {
                            case 'h':
                                if (x == ex)
                                    continue;
                                break;
                            case 'j':
                                if (y == sy)
                                    continue;
                                break;
                            case 'k':
                                if (y == ey)
                                    continue;
                                break;
                            case 'l':
                                if (x == sx)
                                    continue;
                                break;
                            case 'y':
                                if ((y + x) - sumhero >= 1)
                                    continue;
                                break;
                            case 'u':
                                if ((y - x) - diffhero >= 1)
                                    continue;
                                break;
                            case 'n':
                                if ((y + x) - sumhero <= -1)
                                    continue;
                                break;
                            case 'b':
                                if ((y - x) - diffhero <= -1)
                                    continue;
                                break;
                    }
                    switch (ch)
                    {
                        case DOOR:
                            if (x == hero.x || y == hero.y)
                                running = false;
                            break;
                        case PASSAGE:
                            if (x == hero.x || y == hero.y)
                                passcount++;
                            break;
                        case FLOOR:
                        case '|':
                        case '-':
                        case ' ':
                            break;
                        default:
                            running = false;
                            break;
                    }
                }
            }
        if (door_stop && !firstmove && passcount > 1)
            running = false;
        if (!running || !jump)
            mvaddch(hero.y, hero.x, PLAYER);
    }

    // Return the character appropriate for this space, taking into
    // account whether or not the player is tripping.
    int trip_ch(int y, int x, int ch)
    {
        if (on(player, ISHALU) && after)
            switch (ch)
            {
                case FLOOR:
                case ' ':
                case PASSAGE:
                case '-':
                case '|':
                case DOOR:
                case TRAP:
                    break;
                default:
                    if (y != stairs.y || x != stairs.x || !seenstairs)
                        ch = rnd_thing();
                    break;
            }
        return ch;
    }

    // Erase the area shown by a lamp in a dark room.
    void erase_lamp(coord pos, room rp)
    {
        int y, x, ey, sy, ex;

        if (!(see_floor && (rp.r_flags & (ISGONE|ISDARK)) == ISDARK
                && !on(player,ISBLIND)))
            return;

        ey = pos.y + 1;
        ex = pos.x + 1;
        sy = pos.y - 1;
        for (x = pos.x - 1; x <= ex; x++)
            for (y = sy; y <= ey; y++)
            {
                if (y == hero.y && x == hero.x)
                    continue;
                move(y, x);
                if (inch() == FLOOR)
                    addch(' ');
            }
    }

    // Should we show the floor in her room at this time?
    bool show_floor()
    {
        if ((proom.r_flags & (ISGONE | ISDARK)) == ISDARK && !on(player, ISBLIND))
            return see_floor;
        else
            return true;
    }

    // Find the unclaimed object at y, x
    THING find_obj(int y, int x)
    {
        THING obj;

        for (obj = lvl_obj; obj != null; obj = next(obj))
        {
            if (obj.o_pos.y == y && obj.o_pos.x == x)
                return obj;
        }
        /* NOTREACHED */
        return null;
    }

    // She wants to eat something, so let her try
    void eat()
    {
        THING obj;

        if ((obj = get_item("eat", FOOD)) == null)
            return;
        if (obj.o_type != FOOD)
        {
            if (!terse)
                msg("ugh, you would get ill if you ate that");
            else
                msg("that's Inedible!");
            return;
        }
        if (food_left < 0)
            food_left = 0;
        if ((food_left += HUNGERTIME - 200 + rnd(400)) > STOMACHSIZE)
            food_left = STOMACHSIZE;
        hungry_state = 0;
        if (obj == cur_weapon)
            cur_weapon = null;
        if (obj.o_which == 1)
            msg("my, that was a yummy %s", fruit);
        else
            if (rnd(100) > 70)
            {
                pstats.s_exp++;
                msg("{0}, this food tastes awful", choose_str("bummer", "yuk"));
                check_level();
            }
            else
                msg("{0}, that tasted good", choose_str("oh, wow", "yum"));
        leave_pack(obj, false, false);
    }

    // Check to see if the guy has gone up a level.
    void check_level()
    {
        int i, add, olevel;

        for (i = 0; e_levels[i] != 0; i++)
            if (e_levels[i] > pstats.s_exp)
                break;
        i++;
        olevel = pstats.s_lvl;
        pstats.s_lvl = i;
        if (i > olevel)
        {
            add = roll(i - olevel, 10);
            max_hp += add;
            pstats.s_hpt += add;
            msg("welcome to level {0}", i);
        }
    }

    // used to modify the playes strength.  It keeps track of the
    // highest it has been, just in case
    void chg_str(int amt)
    {
        int comp;

        if (amt == 0)
            return;
        int myStr = pstats.s_str;
        add_str(ref myStr, amt);
        pstats.s_str = myStr;
        comp = pstats.s_str;
        if (ISRING(LEFT, R_ADDSTR))
            add_str(ref comp, -cur_ring[LEFT].o_arm);
        if (ISRING(RIGHT, R_ADDSTR))
            add_str(ref comp, -cur_ring[RIGHT].o_arm);
        if (comp > max_stats.s_str)
            max_stats.s_str = comp;
    }

    // Perform the actual add, checking upper and lower bound limits
    void add_str(ref int sp, int amt)
    {
        if ((sp += amt) < 3)
            sp = 3;
        else if (sp > 31)
            sp = 31;
    }

    // Add a haste to the player
    bool add_haste(bool potion)
    {
        if (on(player, ISHASTE))
        {
            no_command += rnd(8);
            player.t_flags &= ~(ISRUN | ISHASTE);
            extinguish(nohaste);
            msg("you faint from exhaustion");
            return false;
        }
        else
        {
            player.t_flags |= ISHASTE;
            if (potion)
                fuse(nohaste, 0, rnd(4) + 4, AFTER);
            return true;
        }
    }

    // Aggravate all the monsters on this level
    void aggravate()
    {
        THING mp;

        for (mp = mlist; mp != null; mp = next(mp))
            runto(mp.t_pos);
    }

    // For printfs: if string starts with a vowel, return "n" for an "an".
    string vowelstr(string str)
    {
        switch (str[0])
        {
        case 'a': case 'A':
        case 'e': case 'E':
        case 'i': case 'I':
        case 'o': case 'O':
        case 'u': case 'U':
            return "n";
        default:
            return "";
        }
    }

    // See if the object is one of the currently used items
    bool is_current(THING obj)
    {
        if (obj == null)
            return false;
        if (obj == cur_armor || obj == cur_weapon || obj == cur_ring[LEFT]
            || obj == cur_ring[RIGHT])
        {
            if (!terse)
                addmsg("That's already ");
            msg("in use");
            return true;
        }
        return false;
    }

    coord last_delt = new coord();

    // Set up the direction co_ordinate for use in varios "prefix" commands
    bool get_dir()
    {
        string prompt;
        bool gotit;

        if (again && last_dir != '\0')
        {
            delta.y = last_delt.y;
            delta.x = last_delt.x;
            dir_ch = last_dir;
        }
        else
        {
            if (!terse)
                msg(prompt = "which direction? ");
            else
                prompt = "direction: ";
            do
            {
                gotit = true;
                switch (dir_ch = readchar())
                {
                    case 'h': case'H': delta.y =  0; delta.x = -1; break;
                    case 'j': case'J': delta.y =  1; delta.x =  0; break;
                    case 'k': case'K': delta.y = -1; delta.x =  0; break;
                    case 'l': case'L': delta.y =  0; delta.x =  1; break;
                    case 'y': case'Y': delta.y = -1; delta.x = -1; break;
                    case 'u': case'U': delta.y = -1; delta.x =  1; break;
                    case 'b': case'B': delta.y =  1; delta.x = -1; break;
                    case 'n': case'N': delta.y =  1; delta.x =  1; break;
                    case (char)ESCAPE: last_dir = '\0'; reset_last(); return false;
                    default:
                        mpos = 0;
                        msg(prompt);
                        gotit = false;
                        break;
                }
            }
            while (!gotit);
            if (char.IsUpper(dir_ch))
                dir_ch = char.ToLower(dir_ch);
            last_dir = dir_ch;
            last_delt.y = delta.y;
            last_delt.x = delta.x;
        }
        if (on(player, ISHUH) && rnd(5) == 0)
        do
        {
            delta.y = rnd(3) - 1;
            delta.x = rnd(3) - 1;
        }
        while (delta.y == 0 && delta.x == 0);
            mpos = 0;
        return true;
    }

    // Give a spread around a given number (+/- 20%)
    int spread(int nm)
    {
        return nm - nm / 20 + rnd(nm / 10);
    }

    // Call an object something after use.
    void call_it(obj_info info)
    {
        if (info.oi_know)
        {
            if (!string.IsNullOrEmpty(info.oi_guess))
            {
                info.oi_guess = null;
            }
        }
        else if (string.IsNullOrEmpty(info.oi_guess))
        {
            msg(terse ? "call it: " : "what do you want to call it? ");

            string buf;
            if (get_str(out buf, stdscr) == NORM)
            {
                if (info.oi_guess != null)
                    info.oi_guess = buf;
            }
        }
    }

    // Pick a random thing appropriate for this level
    char rnd_thing()
    {
        int i;
        char[] thing_list = {
            POTION, SCROLL, RING, STICK, FOOD, WEAPON, ARMOR, STAIRS, GOLD, AMULET
        };

        if (level >= AMULETLEVEL)
            i = rnd(thing_list.Length);
        else
            i = rnd(thing_list.Length - 1);
        return thing_list[i];
    }

    // Choose the first or second string depending on whether it the
    // player is tripping
    string choose_str(string ts, string ns)
    {
        return (on(player, ISHALU) ? ts : ns);
    }
}