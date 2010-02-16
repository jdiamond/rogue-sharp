/*
 * Contains functions for dealing with things like potions, scrolls,
 * and other items.
 *
 * @(#)things.c	4.53 (Berkeley) 02/05/99
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
    // Return the name of something as it would appear in an
    // inventory.
    string inv_name(THING obj, bool drop)
    {
        string pb;
        obj_info op;
        string sp;
        int which;

        pb = "";
        which = obj.o_which;
        switch (obj.o_type)
        {
            case POTION:
                nameit(obj, "potion", p_colors[which], pot_info[which], nullstr);
                pb = prbuf;
                break;
            case RING:
                nameit(obj, "ring", r_stones[which], ring_info[which], ring_num);
                pb = prbuf;
                break;
            case STICK:
                nameit(obj, ws_type[which], ws_made[which], ws_info[which], charge_str);
                pb = prbuf;
                break;
            case SCROLL:
                if (obj.o_count == 1)
                {
                    pb = "A scroll ";
                }
                else
                {
                    pb = string.Format("{0} scrolls ", obj.o_count);
                }
                op = scr_info[which];
                if (op.oi_know)
                    pb += string.Format("of {0}", op.oi_name);
                else if (!string.IsNullOrEmpty(op.oi_guess))
                    pb += string.Format("called {0}", op.oi_guess);
                else
                    pb += string.Format("titled '{0}'", s_names[which]);
                break;
            case FOOD:
                if (which == 1)
                    if (obj.o_count == 1)
                        pb = string.Format("A{0} {1}", vowelstr(fruit), fruit);
                    else
                        pb = string.Format("{0} {1}s", obj.o_count, fruit);
                else
                    if (obj.o_count == 1)
                        pb = "Some food";
                    else
                        pb = string.Format("{0} rations of food", obj.o_count);
                break;
            case WEAPON:
                sp = weap_info[which].oi_name;
                if (obj.o_count > 1)
                    pb = string.Format("{0} ", obj.o_count);
                else
                    pb = string.Format("A{0} ", vowelstr(sp));
                if ((obj.o_flags & ISKNOW) != 0)
                    pb += string.Format("{0} {1}", num(obj.o_hplus, obj.o_dplus, WEAPON), sp);
                else
                    pb += sp;
                if (obj.o_count > 1)
                    pb += "s";
                if (obj.o_label != null)
                {
                    pb += string.Format(" called {0}", obj.o_label);
                }
                break;
            case ARMOR:
                sp = arm_info[which].oi_name;
                if ((obj.o_flags & ISKNOW) != 0)
                {
                    pb = string.Format("{0} {1} [", num(a_class[which] - obj.o_arm, 0, ARMOR), sp);
                    if (!terse)
                        pb += "protection ";
                    pb += string.Format("{0}]", 10 - obj.o_arm);
                }
                else
                    pb = sp;
                if (obj.o_label != null)
                {
                    pb += string.Format(" called {0}", obj.o_label);
                }
                break;
            case AMULET:
                pb = "The Amulet of Yendor";
                break;
            case GOLD:
                pb = string.Format("{0} Gold pieces", obj.o_goldval);
                break;
        }
        if (inv_describe)
        {
            if (obj == cur_armor)
                pb += " (being worn)";
            if (obj == cur_weapon)
                pb += " (weapon in hand)";
            if (obj == cur_ring[LEFT])
                pb += " (on left hand)";
            else if (obj == cur_ring[RIGHT])
                pb += " (on right hand)";
        }
        if (drop && char.IsUpper(pb[0]))
            pb = pb.Substring(0, 1).ToLower() + pb.Substring(1);
        else if (!drop && char.IsUpper(pb[0]))
            pb = pb.Substring(0, 1).ToUpper() + pb.Substring(1);

        return pb;
    }

    // Put something down
    void drop()
    {
        char ch;
        THING obj;

        ch = chat(hero.y, hero.x);
        if (ch != FLOOR && ch != PASSAGE)
        {
            after = false;
            msg("there is something there already");
            return;
        }
        if ((obj = get_item("drop", 0)) == null)
            return;
        if (!dropcheck(obj))
            return;
        obj = leave_pack(obj, true, (bool)!ISMULT(obj.o_type));
        /*
         * Link it into the level object list
         */
        attach(ref lvl_obj, obj);
        chat(hero.y, hero.x, (char)obj.o_type);
        flat(hero.y, hero.x, flat(hero.y, hero.x) | F_DROPPED);
        obj.o_pos = hero;
        if (obj.o_type == AMULET)
            amulet = false;
        msg("dropped {0}", inv_name(obj, true));
    }

    // Do special checks for dropping or unweilding|unwearing|unringing
    bool dropcheck(THING obj)
    {
        if (obj == null)
            return true;
        if (obj != cur_armor && obj != cur_weapon
        && obj != cur_ring[LEFT] && obj != cur_ring[RIGHT])
            return true;
        if ((obj.o_flags & ISCURSED) != 0)
        {
            msg("you can't.  It appears to be cursed");
            return false;
        }
        if (obj == cur_weapon)
            cur_weapon = null;
        else if (obj == cur_armor)
        {
            waste_time();
            cur_armor = null;
        }
        else
        {
            cur_ring[obj == cur_ring[LEFT] ? LEFT : RIGHT] = null;
            switch (obj.o_which)
            {
                case R_ADDSTR:
                    chg_str(-obj.o_arm);
                    break;
                case R_SEEINVIS:
                    unsee(0);
                    extinguish(unsee);
                    break;
            }
        }
        return true;
    }

    // Return a new thing
    THING new_thing()
    {
        THING cur;
        int r;

        cur = new_item();
        cur.o_hplus = 0;
        cur.o_dplus = 0;
        cur.o_damage = "0x0";
        cur.o_hurldmg = "0x0";
        cur.o_arm = 11;
        cur.o_count = 1;
        cur.o_group = 0;
        cur.o_flags = 0;
        /*
         * Decide what kind of object it will be
         * If we haven't had food for a while, let it be food.
         */
        switch (no_food > 3 ? 2 : pick_one(things, NUMTHINGS))
        {
            case 0:
                cur.o_type = POTION;
                cur.o_which = pick_one(pot_info, MAXPOTIONS);
                break;
            case 1:
                cur.o_type = SCROLL;
                cur.o_which = pick_one(scr_info, MAXSCROLLS);
                break;
            case 2:
                cur.o_type = FOOD;
                no_food = 0;
                if (rnd(10) != 0)
                    cur.o_which = 0;
                else
                    cur.o_which = 1;
                break;
            case 3:
                init_weapon(cur, pick_one(weap_info, MAXWEAPONS));
                if ((r = rnd(100)) < 10)
                {
                    cur.o_flags |= ISCURSED;
                    cur.o_hplus -= rnd(3) + 1;
                }
                else if (r < 15)
                    cur.o_hplus += rnd(3) + 1;
                break;
            case 4:
                cur.o_type = ARMOR;
                cur.o_which = pick_one(arm_info, MAXARMORS);
                cur.o_arm = a_class[cur.o_which];
                if ((r = rnd(100)) < 20)
                {
                    cur.o_flags |= ISCURSED;
                    cur.o_arm += rnd(3) + 1;
                }
                else if (r < 28)
                    cur.o_arm -= rnd(3) + 1;
                break;
            case 5:
                cur.o_type = RING;
                cur.o_which = pick_one(ring_info, MAXRINGS);
                switch (cur.o_which)
                {
                    case R_ADDSTR:
                    case R_PROTECT:
                    case R_ADDHIT:
                    case R_ADDDAM:
                        if ((cur.o_arm = rnd(3)) == 0)
                        {
                            cur.o_arm = -1;
                            cur.o_flags |= ISCURSED;
                        }
                        break;
                    case R_AGGR:
                    case R_TELEPORT:
                        cur.o_flags |= ISCURSED;
                        break;
                }
                break;
            case 6:
                cur.o_type = STICK;
                cur.o_which = pick_one(ws_info, MAXSTICKS);
                fix_stick(cur);
                break;
        }
        return cur;
    }

    // Pick an item out of a list of nitems possible objects
    int pick_one(obj_info[] info, int nitems)
    {
        int i;

        i = rnd(100);
        int j = 0;
        foreach (var item in info)
        {
            if (i < item.oi_prob)
                return j;
            j++;
        }
        return 0;
    }

    static int line_cnt = 0;
    static bool newpage = false;
    static string lastfmt, lastarg;

    // list what the player has discovered in this game of a certain type
    void discovered()
    {
        char ch;
        bool disc_list;

        do
        {
            disc_list = false;
            if (!terse)
                addmsg("for ");
            addmsg("what type");
            if (!terse)
                addmsg(" of object do you want a list");
            msg("? (* for all)");
            ch = readchar();
            switch (ch)
            {
                case (char)ESCAPE:
                    msg("");
                    return;
                case POTION:
                case SCROLL:
                case RING:
                case STICK:
                case '*':
                    disc_list = true;
                    break;
                default:
                    if (terse)
                        msg("Not a type");
                    else
                        msg("Please type one of {0}{1}{2}{3} (ESCAPE to quit)", POTION, SCROLL, RING, STICK);
                    break;
            }
        } while (!disc_list);
        if (ch == '*')
        {
            print_disc(POTION);
            add_line("", null);
            print_disc(SCROLL);
            add_line("", null);
            print_disc(RING);
            add_line("", null);
            print_disc(STICK);
            end_line();
        }
        else
        {
            print_disc(ch);
            end_line();
        }
    }

    // Print what we've discovered of type 'type'
    void print_disc(char type)
    {
        obj_info[] info = null;
        int i, maxnum = 0, num_found;
        THING obj = new THING();
        int[] order = new int[Math.Max(MAXSCROLLS, Math.Max(MAXPOTIONS, Math.Max(MAXRINGS, MAXSTICKS)))];

        switch (type)
        {
            case SCROLL:
                maxnum = MAXSCROLLS;
                info = scr_info;
                break;
            case POTION:
                maxnum = MAXPOTIONS;
                info = pot_info;
                break;
            case RING:
                maxnum = MAXRINGS;
                info = ring_info;
                break;
            case STICK:
                maxnum = MAXSTICKS;
                info = ws_info;
                break;
        }
        set_order(order, maxnum);
        obj.o_count = 1;
        obj.o_flags = 0;
        num_found = 0;
        for (i = 0; i < maxnum; i++)
            if (info[order[i]].oi_know || !string.IsNullOrEmpty(info[order[i]].oi_guess))
            {
                obj.o_type = type;
                obj.o_which = order[i];
                add_line("{0}", inv_name(obj, false));
                num_found++;
            }
        if (num_found == 0)
            add_line(nothing(type), null);
    }

    // Set up order for list
    void set_order(int[] order, int numthings)
    {
        int i, r, t;

        for (i = 0; i < numthings; i++)
            order[i] = i;

        for (i = numthings; i > 0; i--)
        {
            r = rnd(i);
            t = order[i - 1];
            order[i - 1] = order[r];
            order[r] = t;
        }
    }

    private static int maxlen = -1;

    // Add a line to the list of discoveries
    int add_line(string fmt, string arg)
    {
        IntPtr tw, sw;
        int x, y;
        string prompt = "--Press space to continue--";

        if (line_cnt == 0)
        {
            wclear(hw);
            if (inv_type == INV_SLOW)
                mpos = 0;
        }
        if (inv_type == INV_SLOW)
        {
            if (!string.IsNullOrEmpty(fmt))
                if (msg(fmt, arg) == ESCAPE)
                    return ESCAPE;
            line_cnt++;
        }
        else
        {
            if (maxlen < 0)
                maxlen = prompt.Length;
            if (line_cnt >= LINES - 1 || fmt == null)
            {
                if (inv_type == INV_OVER && fmt == null && !newpage)
                {
                    msg("");
                    refresh();
                    tw = newwin(line_cnt + 1, maxlen + 2, 0, COLS - maxlen - 3);
                    sw = subwin(tw, line_cnt + 1, maxlen + 1, 0, COLS - maxlen - 2);
                    for (y = 0; y <= line_cnt; y++)
                    {
                        wmove(sw, y, 0);
                        for (x = 0; x <= maxlen; x++)
                            waddch(sw, mvwinch(hw, y, x));
                    }
                    wmove(tw, line_cnt, 1);
                    waddstr(tw, prompt);
                    /*
                     * if there are lines below, use 'em
                     */
                    if (LINES > NUMLINES)
                    {
                        if (NUMLINES + line_cnt > LINES)
                            mvwin(tw, LINES - (line_cnt + 1), COLS - maxlen - 3);
                        else
                            mvwin(tw, NUMLINES, 0);
                    }
                    touchwin(tw);
                    wrefresh(tw);
                    wait_for(' ');
                    if (true) // md_hasclreol()
                    {
                        werase(tw);
                        leaveok(tw, true);
                        wrefresh(tw);
                    }
                    delwin(tw);
                    touchwin(stdscr);
                }
                else
                {
                    wmove(hw, LINES - 1, 0);
                    waddstr(hw, prompt);
                    wrefresh(hw);
                    wait_for(' ');
                    clearok(curscr, true);
                    wclear(hw);
                    touchwin(stdscr);
                }
                newpage = true;
                line_cnt = 0;
                maxlen = prompt.Length;
            }
            if (fmt != null && !(line_cnt == 0 && fmt.Length == 0))
            {
                mvwprintw(hw, line_cnt++, 0, fmt, arg);
                getyx(hw, out y, out x);
                if (maxlen < x)
                    maxlen = x;
                lastfmt = fmt;
                lastarg = arg;
            }
        }
        return ~ESCAPE;
    }

    // End the list of lines
    void end_line()
    {
        if (inv_type != INV_SLOW)
        {
            if (line_cnt == 1 && !newpage)
            {
                mpos = 0;
                msg(lastfmt, lastarg);
            }
            else
                add_line(null, null);
        }
        line_cnt = 0;
        newpage = false;
    }

    // Set up prbuf so that message for "nothing found" is there
    string nothing(char type)
    {
        string sp, tystr = null;

        if (terse)
            sp = "Nothing";
        else
            sp = "Haven't discovered anything";
        if (type != '*')
        {
            switch (type)
            {
                case POTION: tystr = "potion"; break;
                case SCROLL: tystr = "scroll"; break;
                case RING: tystr = "ring"; break;
                case STICK: tystr = "stick"; break;
            }
            return sp + string.Format(" about any {0}s", tystr);
        }
        return prbuf;
    }

    // Give the proper name to a potion, stick, or ring
    void nameit(THING obj, string type, string which, obj_info op, Func<THING, string> prfunc)
    {
        if (op.oi_know || !string.IsNullOrEmpty(op.oi_guess))
        {
            if (obj.o_count == 1)
                prbuf = string.Format("A {0} ", type);
            else
                prbuf = string.Format("{0} {1}s ", obj.o_count, type);
            if (op.oi_know)
                prbuf += string.Format("of {0}{1}({2})", op.oi_name, prfunc(obj), which);
            else if (!string.IsNullOrEmpty(op.oi_guess))
                prbuf += string.Format("called {0}{1}({2})", op.oi_guess, prfunc(obj), which);
        }
        else if (obj.o_count == 1)
            prbuf = string.Format("A{0} {1} {2}", vowelstr(which), which, type);
        else
            prbuf = string.Format("{0} {1} {2}s", obj.o_count, which, type);
    }

    // Return a pointer to a null-length string
    string nullstr(THING ignored)
    {
        return "";
    }
}