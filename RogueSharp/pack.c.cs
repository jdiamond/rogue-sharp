/*
 * Routines to deal with the pack
 *
 * @(#)pack.c	4.40 (Berkeley) 02/05/99
 *
 * Rogue: Exploring the Dungeons of Doom
 * Copyright (C) 1980-1983, 1985, 1999 Michael Toy, Ken Arnold and Glenn Wichman
 * All rights reserved.
 *
 * See the file LICENSE.TXT for full copyright and licensing information.
 */

partial class Rogue
{
    // Pick up an object and add it to the pack.  If the argument is
    // non-null use it as the linked_list pointer instead of gettting
    // it off the ground.
    void add_pack(THING obj, bool silent)
    {
        THING op, lp;
        bool from_floor;

        from_floor = false;
        if (obj == null)
        {
            if ((obj = find_obj(hero.y, hero.x)) == null)
                return;
            from_floor = true;
        }

        /*
         * Check for and deal with scare monster scrolls
         */
        if (obj.o_type == SCROLL && obj.o_which == S_SCARE)
            if ((obj.o_flags & ISFOUND) == ISFOUND)
            {
                detach(ref lvl_obj, obj);
                mvaddch(hero.y, hero.x, floor_ch());
                chat(hero.y, hero.x, (proom.r_flags & ISGONE) == ISGONE ? PASSAGE : FLOOR);
                discard(obj);
                msg("the scroll turns to dust as you pick it up");
                return;
            }

        if (pack == null)
        {
            pack = obj;
            obj.o_packch = pack_char();
            inpack++;
        }
        else
        {
            lp = null;
            for (op = pack; op != null; op = next(op))
            {
                if (op.o_type != obj.o_type)
                    lp = op;
                else
                {
                    while (op.o_type == obj.o_type && op.o_which != obj.o_which)
                    {
                        lp = op;
                        if (next(op) == null)
                            break;
                        else
                            op = next(op);
                    }
                    if (op.o_type == obj.o_type && op.o_which == obj.o_which)
                    {
                        if (ISMULT(op.o_type))
                        {
                            if (!pack_room(from_floor, obj))
                                return;
                            op.o_count++;
                            //dump_it:
                            discard(obj);
                            obj = op;
                            lp = null;
                            goto break_out;
                        }
                        else if (obj.o_group != 0)
                        {
                            lp = op;
                            while (op.o_type == obj.o_type
                                && op.o_which == obj.o_which
                                && op.o_group != obj.o_group)
                            {
                                lp = op;
                                if (next(op) == null)
                                    break;
                                else
                                    op = next(op);
                            }
                            if (op.o_type == obj.o_type
                                && op.o_which == obj.o_which
                                && op.o_group == obj.o_group)
                            {
                                op.o_count += obj.o_count;
                                inpack--;
                                if (!pack_room(from_floor, obj))
                                    return;
                                //goto dump_it;
                                discard(obj);
                                obj = op;
                                lp = null;
                                goto break_out;
                            }
                        }
                        else
                            lp = op;
                    }
                break_out:
                    break;
                }
            }

            if (lp != null)
            {
                if (!pack_room(from_floor, obj))
                    return;
                else
                {
                    obj.o_packch = pack_char();
                    obj.l_next = next(lp);
                    obj.l_prev = lp;
                    if (next(lp) != null)
                        next(lp).l_prev = obj;
                    lp.l_next = obj;
                }
            }
        }

        obj.o_flags |= ISFOUND;

        /*
         * If this was the object of something's desire, that monster will
         * get mad and run at the hero.
         */
        for (op = mlist; op != null; op = next(op))
            if (op.t_dest.Equals(obj.o_pos))
                op.t_dest = hero;

        if (obj.o_type == AMULET)
            amulet = true;
        /*
         * Notify the user
         */
        if (!silent)
        {
            if (!terse)
                addmsg("you now have ");
            msg("{0} ({1})", inv_name(obj, !terse), obj.o_packch);
        }
    }

    // See if there's room in the pack.  If not, print out an
    // appropriate message
    bool pack_room(bool from_floor, THING obj)
    {
        if (++inpack > MAXPACK)
        {
            if (!terse)
                addmsg("there's ");
            addmsg("no room");
            if (!terse)
                addmsg(" in your pack");
            endmsg();
            if (from_floor)
                move_msg(obj);
            inpack = MAXPACK;
            return false;
        }

        if (from_floor)
        {
            detach(ref lvl_obj, obj);
            mvaddch(hero.y, hero.x, floor_ch());
            chat(hero.y, hero.x, (proom.r_flags & ISGONE) == ISGONE ? PASSAGE : FLOOR);
        }

        return true;
    }

    // take an item out of the pack
    THING leave_pack(THING obj, bool newobj, bool all)
    {
        THING nobj;

        inpack--;
        nobj = obj;
        if (obj.o_count > 1 && !all)
        {
            last_pick = obj;
            obj.o_count--;
            if (obj.o_group != 0)
                inpack++;
            if (newobj)
            {
                nobj = obj.Copy();
                nobj.l_next = null;
                nobj.l_prev = null;
                nobj.o_count = 1;
            }
        }
        else
        {
            last_pick = null;
            pack_used[obj.o_packch - 'a'] = false;
            THING myPack = pack;
            detach(ref myPack, obj);
            pack = myPack;
        }
        return nobj;
    }

    // Return the next unused pack character.
    char pack_char()
    {
        int i;

        for (i = 0; pack_used[i]; i++)
            continue;
        pack_used[i] = true;
        return (char)(i + 'a');
    }

    string inv_temp;

    // List what is in the pack.  Return TRUE if there is something of
    // the given type.
    bool inventory(THING list, int type)
    {
        n_objs = 0;
        for (; list != null; list = next(list))
        {
            if (type != 0 && type != list.o_type && !(type == CALLABLE &&
                list.o_type != FOOD && list.o_type != AMULET) &&
                !(type == R_OR_S && (list.o_type == RING || list.o_type == STICK)))
                continue;
            n_objs++;
            inv_temp = string.Format("{0}) {{0}}", list.o_packch);
            msg_esc = true;
            if (add_line(inv_temp, inv_name(list, false)) == ESCAPE)
            {
                msg_esc = false;
                msg("");
                return true;
            }
            msg_esc = false;
        }
        if (n_objs == 0)
        {
            if (terse)
                msg(type == 0 ? "empty handed" :
                        "nothing appropriate");
            else
                msg(type == 0 ? "you are empty handed" :
                        "you don't have anything appropriate");
            return false;
        }
        end_line();
        return true;
    }

    // Add something to characters pack.
    void pick_up(char ch)
    {
        THING obj;

        if (on(player, ISLEVIT))
            return;

        obj = find_obj(hero.y, hero.x);
        if (move_on)
            move_msg(obj);
        else
            switch (ch)
            {
                case GOLD:
                    if (obj == null)
                        return;
                    money(obj.o_goldval);
                    detach(ref lvl_obj, obj);
                    discard(obj);
                    proom.r_goldval = 0;
                    break;
                default:
                case ARMOR:
                case POTION:
                case FOOD:
                case WEAPON:
                case SCROLL:
                case AMULET:
                case RING:
                case STICK:
                    add_pack(null, false);
                    break;
            }
    }

    // Print out the message if you are just moving onto an object
    void move_msg(THING obj)
    {
        if (!terse)
            addmsg("you ");
        msg("moved onto {0}", inv_name(obj, true));
    }

    // Allow player to inventory a single item
    void picky_inven()
    {
        THING obj;
        char mch;

        if (pack == null)
            msg("you aren't carrying anything");
        else if (next(pack) == null)
            msg("a) {0}", inv_name(pack, false));
        else
        {
            msg(terse ? "item: " : "which item do you wish to inventory: ");
            mpos = 0;
            if ((mch = readchar()) == ESCAPE)
            {
                msg("");
                return;
            }
            for (obj = pack; obj != null; obj = next(obj))
                if (mch == obj.o_packch)
                {
                    msg("{0}) {1}", mch, inv_name(obj, false));
                    return;
                }
            msg("'{0}' not in pack", unctrl(mch));
        }
    }

    // get_item:
    THING get_item(string purpose, int type)
    {
        THING obj;
        char ch;

        if (pack == null)
            msg("you aren't carrying anything");
        else if (again)
            if (last_pick != null)
                return last_pick;
            else
                msg("you ran out");
        else
        {
            for (; ; )
            {
                if (!terse)
                    addmsg("which object do you want to ");
                addmsg(purpose);
                if (terse)
                    addmsg(" what");
                msg("? (* for list): ");
                ch = readchar();
                mpos = 0;
                /*
                 * Give the poor player a chance to abort the command
                 */
                if (ch == ESCAPE)
                {
                    reset_last();
                    after = false;
                    msg("");
                    return null;
                }
                n_objs = 1;		/* normal case: person types one char */
                if (ch == '*')
                {
                    mpos = 0;
                    if (!inventory(pack, type))
                    {
                        after = false;
                        return null;
                    }
                    continue;
                }
                for (obj = pack; obj != null; obj = next(obj))
                    if (obj.o_packch == ch)
                        break;
                if (obj == null)
                {
                    msg("'{0}' is not a valid item", unctrl(ch));
                    continue;
                }
                else
                    return obj;
            }
        }
        return null;
    }

    // Add or subtract gold from the pack
    void money(int value)
    {
        purse += value;
        mvaddch(hero.y, hero.x, floor_ch());
        chat(hero.y, hero.x, ((proom.r_flags & ISGONE) != 0) ? PASSAGE : FLOOR);
        if (value > 0)
        {
            if (!terse)
                addmsg("you found ");
            msg("{0} gold pieces", value);
        }
    }

    // Return the appropriate floor character for her room
    char floor_ch()
    {
        if ((proom.r_flags & ISGONE) == ISGONE)
            return PASSAGE;
        return (show_floor() ? FLOOR : ' ');
    }

    // Return the character at hero's position, taking see_floor
    // into account
    char floor_at()
    {
        char ch;

        ch = chat(hero.y, hero.x);
        if (ch == FLOOR)
            ch = floor_ch();
        return ch;
    }

    // Reset the last command when the current one is aborted
    void reset_last()
    {
        last_comm = l_last_comm;
        last_dir = l_last_dir;
        last_pick = l_last_pick;
    }
}