/*
 * Read and execute the user commands
 *
 * @(#)command.c	4.73 (Berkeley) 08/06/83
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
    private char countch, direction;
    bool newcount = false;

    // Process the user commands
    void command()
    {
        char ch;
        int ntimes = 1;			/* Number of player moves */
        int fp;
        THING mp;

        if (on(player, ISHASTE))
            ntimes++;
        /*
         * Let the daemons start up
         */
        do_daemons(BEFORE);
        do_fuses(BEFORE);
        while (ntimes-- != 0)
        {
            again = false;
            if (has_hit)
            {
                endmsg();
                has_hit = false;
            }
            /*
             * these are illegal things for the player to be, so if any are
             * set, someone's been poking in memeory
             */
            if (on(player, ISSLOW|ISGREED|ISINVIS|ISREGEN|ISTARGET))
                Environment.Exit(1);

            look(true);
            if (!running)
                door_stop = false;
            status();
            lastscore = purse;
            move(hero.y, hero.x);
            if (!((running || count != 0) && jump))
                refresh();			/* Draw screen */
            take = '\0';
            after = true;
            /*
             * Read command or continue run
             */
            if (no_command == 0)
            {
                if (running || to_death)
                    ch = runch;
                else if (count != 0)
                    ch = countch;
                else
                {
                    ch = readchar();
                    move_on = false;
                    if (mpos != 0)		/* Erase message if its there */
                        msg("");
                }
            }
            else
                ch = '.';
            if (no_command != 0)
            {
                if (--no_command == 0)
                {
                    player.t_flags |= ISRUN;
                    msg("you can move again");
                }
            }
            else
            {
                /*
                 * check for prefixes
                 */
                newcount = false;
                if (char.IsDigit(ch))
                {
                    count = 0;
                    newcount = true;
                    while (char.IsDigit(ch))
                    {
                        count = count * 10 + (ch - '0');
                        if (count > 255)
                            count = 255;
                        ch = readchar();
                    }
                    countch = ch;
                    /*
                     * turn off count for commands which don't make sense
                     * to repeat
                     */
                    switch (ch)
                    {
                        case (char)2: case (char)8: case (char)10:
                        case (char)11: case (char)12: case (char)14:
                        case (char)21: case (char)25:
                        case '.': case 'a': case 'b': case 'h': case 'j':
                        case 'k': case 'l': case 'm': case 'n': case 'q':
                        case 'r': case 's': case 't': case 'u': case 'y':
                        case 'z': case 'B': case 'C': case 'H': case 'I':
                        case 'J': case 'K': case 'L': case 'N': case 'U':
                        case 'Y':
                        break;
                        default:
                            count = 0;
                            break;
                    }
                }
                /*
                 * execute a command
                 */
                if (count != 0 && !running)
                    count--;
                if (ch != 'a' && ch != ESCAPE && !(running || (count != 0) || to_death))
                {
                    l_last_comm = last_comm;
                    l_last_dir = last_dir;
                    l_last_pick = last_pick;
                    last_comm = ch;
                    last_dir = '\0';
                    last_pick = null;
                }
        over:
                switch (ch)
                {
                    case ',':
                        {
                            THING obj = null;
                            int found = 0;
                            for (obj = lvl_obj; obj != null; obj = next(obj))
                            {
                                if (obj.o_pos.y == hero.y && obj.o_pos.x == hero.x)
                                {
                                    found=1;
                                    break;
                                }
                            }
                            if (found != 0)
                            {
                                if (!levit_check())
                                    pick_up((char)obj.o_type);
                            }
                            else
                            {
                                if (!terse)
                                    addmsg("there is ");
                                addmsg("nothing here");
                                if (!terse)
                                    addmsg(" to pick up");
                                endmsg();
                            }
                            break;
                        }
                    case 'h': do_move(0, -1); break;
                    case 'j': do_move(1, 0); break;
                    case 'k': do_move(-1, 0); break;
                    case 'l': do_move(0, 1); break;
                    case 'y': do_move(-1, -1); break;
                    case 'u': do_move(-1, 1); break;
                    case 'b': do_move(1, -1); break;
                    case 'n': do_move(1, 1); break;
                    case 'H': do_run('h'); break;
                    case 'J': do_run('j'); break;
                    case 'K': do_run('k'); break;
                    case 'L': do_run('l'); break;
                    case 'Y': do_run('y'); break;
                    case 'U': do_run('u'); break;
                    case 'B': do_run('b'); break;
                    case 'N': do_run('n'); break;
                    case (char)8: case (char)10: case (char)11: case (char)12:
                    case (char)25: case (char)21: case (char)2: case (char)14:
                        {
                            if (!on(player, ISBLIND))
                            {
                                door_stop = true;
                                firstmove = true;
                            }
                            if (count != 0 && !newcount)
                                ch = direction;
                            else
                            {
                                ch = (char)(ch + ('A' - CTRL('A')));
                                direction = ch;
                            }
                            goto over;
                        }
                    case 'F':
                        kamikaze = true;
                        goto case 'f';
                    case 'f':
                        if (!get_dir())
                        {
                            after = false;
                            break;
                        }
                        delta.y += hero.y;
                        delta.x += hero.x;
                        if (((mp = moat(delta.y, delta.x)) == null) || ((!see_monst(mp)) && !on(player, SEEMONST)))
                        {
                            if (!terse)
                                addmsg("I see ");
                            msg("no monster there");
                            after = false;
                        }
                        else if (diag_ok(hero, delta))
                        {
                            to_death = true;
                            max_hit = 0;
                            mp.t_flags |= ISTARGET;
                            runch = ch = dir_ch;
                            goto over;
                        }
                        break;
                    case 't':
                        if (!get_dir())
                            after = false;
                        else
                            missile(delta.y, delta.x);
                        break;
                    case 'a':
                        if (last_comm == '\0')
                        {
                            msg("you haven't typed a command yet");
                            after = false;
                        }
                        else
                        {
                            ch = last_comm;
                            again = true;
                            goto over;
                        }
                        break;
                    case 'q': quaff(); break;
                    case 'Q':
                        after = false;
                        q_comm = true;
                        quit(0);
                        q_comm = false;
                        break;
                    case 'i': after = false; inventory(pack, 0); break;
                    case 'I': after = false; picky_inven(); break;
                    case 'd': drop(); break;
                    case 'r': read_scroll(); break;
                    case 'e': eat(); break;
                    case 'w': wield(); break;
                    case 'W': wear(); break;
                    case 'T': take_off(); break;
                    case 'P': ring_on(); break;
                    case 'R': ring_off(); break;
                    //when 'o': option(); after = FALSE;
                    case 'c': call(); after = false; break;
                    case '>': after = false; d_level(); break;
                    case '<': after = false; u_level(); break;
                    case '?': after = false; help(); break;
                    case '/': after = false; identify(); break;
                    case 's': search(); break;
                    case 'z':
                        if (get_dir())
                            do_zap();
                        else
                            after = false;
                        break;
                    case 'D': after = false; discovered(); break;
                    case (char)16: after = false; msg(huh); break; // Ctrl+P
                    //when CTRL('R'):
                    //    after = FALSE;
                    //    clearok(curscr,TRUE);
                    //    wrefresh(curscr);
                    //when 'v':
                    //    after = FALSE;
                    //    msg("version %s. (mctesq was here)", release);
                    //when 'S': 
                    //    after = FALSE;
                    //    save_game();
                    case '.': break;			/* Rest command */
                    case ' ': after = false; break;	/* "Legal" illegal command */
                    case '^':
                        after = false;
                        if (get_dir())
                        {
                            delta.y += hero.y;
                            delta.x += hero.x;
                            fp = flat(delta.y, delta.x);
                            if (!terse)
                                addmsg("You have found ");
                            if (chat(delta.y, delta.x) != TRAP)
                                msg("no trap there");
                            else if (on(player, ISHALU))
                                msg(tr_name[rnd(NTRAPS)]);
                            else
                            {
                                msg(tr_name[fp & F_TMASK]);
                                fp |= F_SEEN;
                                flat(delta.y, delta.x, fp);
                            }
                        }
                        break;
                    case (char)ESCAPE:	/* Escape */
                        door_stop = false;
                        count = 0;
                        after = false;
                        again = false;
                        break;
                    case 'm':
                        move_on = true;
                        if (!get_dir())
                            after = false;
                        else
                        {
                            ch = dir_ch;
                            countch = dir_ch;
                            goto over;
                        }
                        break;
                    case ')': current(cur_weapon, "wielding", null); break;
                    case ']': current(cur_armor, "wearing", null); break;
                    case '=':
                        current(cur_ring[LEFT], "wearing",
                                    terse ? "(L)" : "on left hand");
                        current(cur_ring[RIGHT], "wearing",
                                    terse ? "(R)" : "on right hand");
                        break;
                    case '@':
                        stat_msg = true;
                        status();
                        stat_msg = false;
                        after = false;
                        break;
                    default:
                        after = false;
                        if (wizard)
                            switch (ch)
                            {
                                //case '|': msg("@ %d,%d", hero.y, hero.x);
                                //when 'C': create_obj();
                                //when '$': msg("inpack = %d", inpack);
                                //when CTRL('G'): inventory(lvl_obj, 0);
                                //when CTRL('W'): whatis(FALSE, 0);
                                case (char)4: level++; new_level(); break; // Ctrl+D
                                case (char)1: level--; new_level(); break; // Ctrl+A
                                case (char)6: show_map(); break; // Ctrl+F
                                case (char)20: teleport(); break; // Ctrl+T
                                //when CTRL('E'): msg("food left: %d", food_left);
                                //when CTRL('C'): add_pass();
                                case (char)24: turn_see(on(player, SEEMONST) ? 1 : 0); break; // Ctrl+X
                                //when CTRL('~'):
                                //{
                                //    THING *item;
                                //    if ((item = get_item("charge", STICK)) != NULL)
                                //    item->o_charges = 10000;
                                //}
                                case (char)9: // CTRL('I'):
                                    {
                                        int i;
                                        THING obj;

                                        for (i = 0; i < 9; i++)
                                            raise_level();
                                        ///*
                                        // * Give him a sword (+1,+1)
                                        // */
                                        //obj = new_item();
                                        //init_weapon(obj, TWOSWORD);
                                        //obj->o_hplus = 1;
                                        //obj->o_dplus = 1;
                                        //add_pack(obj, TRUE);
                                        //cur_weapon = obj;
                                        ///*
                                        // * And his suit of armor
                                        // */
                                        //obj = new_item();
                                        //obj->o_type = ARMOR;
                                        //obj->o_which = PLATE_MAIL;
                                        //obj->o_arm = -5;
                                        //obj->o_flags |= ISKNOW;
                                        //obj->o_count = 1;
                                        //obj->o_group = 0;
                                        //cur_armor = obj;
                                        //add_pack(obj, TRUE);
                                    }
                                    break;
                                //when '*' :
                                //  pr_list();
                                default:
                                    illcom(ch);
                                    break;
                            }
                        else
                            illcom(ch);
                        break;
                }
                /*
                 * turn off flags if no longer needed
                 */
                if (!running)
                    door_stop = false;
            }
            /*
             * If he ran into something to take, let him pick it up.
             */
            if (take != 0)
                pick_up(take);
            if (!running)
                door_stop = false;
            if (!after)
                ntimes++;
        }
        do_daemons(AFTER);
        do_fuses(AFTER);
        if (ISRING(LEFT, R_SEARCH))
            search();
        else if (ISRING(LEFT, R_TELEPORT) && rnd(50) == 0)
            teleport();
        if (ISRING(RIGHT, R_SEARCH))
            search();
        else if (ISRING(RIGHT, R_TELEPORT) && rnd(50) == 0)
            teleport();
    }

    // What to do with an illegal command
    void illcom(int ch)
    {
        save_msg = false;
        count = 0;
        msg("illegal command '{0}'", unctrl((char)ch));
        save_msg = true;
    }

    // player gropes about him to find hidden things.
    void search()
    {
        int y, x;
        int fp;
        int ey, ex;
        int probinc;
        bool found;

        ey = hero.y + 1;
        ex = hero.x + 1;
        probinc = (on(player, ISHALU) ? 3 : 0);
        probinc += (on(player, ISBLIND) ? 2 : 0);
        found = false;
        for (y = hero.y - 1; y <= ey; y++)
            for (x = hero.x - 1; x <= ex; x++)
            {
                if (y == hero.y && x == hero.x)
                    continue;
                fp = flat(y, x);
                if (!((fp & F_REAL) != 0))
                    switch (chat(y, x))
                    {
                        case '|':
                        case '-':
                            if (rnd(5 + probinc) != 0)
                                break;
                            chat(y, x, DOOR);
                            msg("a secret door");
                    foundone:
                            found = true;
                            fp |= F_REAL;
                            flat(y, x, fp);
                            count = 0;
                            running = false;
                            break;
                        case FLOOR:
                            if (rnd(2 + probinc) != 0)
                                break;
                            chat(y, x, TRAP);
                            if (!terse)
                                addmsg("you found ");
                            if (on(player, ISHALU))
                                msg(tr_name[rnd(NTRAPS)]);
                            else
                            {
                                msg(tr_name[fp & F_TMASK]);
                                fp |= F_SEEN;
                                flat(y, x, fp);
                            }
                            goto foundone;
                        case ' ':
                            if (rnd(3 + probinc) != 0)
                                break;
                            chat(y, x, PASSAGE);
                            goto foundone;
                    }
            }
            if (found)
                look(false);
    }

    // Give single character help, or the whole mess if he wants it
    void help()
    {
        char helpch;
        int numprint, cnt;
        msg("character you want help for (* for all): ");
        helpch = readchar();
        mpos = 0;
        /*
         * If its not a *, print the right help string
         * or an error if he typed a funny character.
         */
        if (helpch != '*')
        {
            move(0, 0);
            foreach (var strp in helpstr)
                if (strp.h_ch == helpch)
                {
                    lower_msg = true;
                    msg("{0}{1}", unctrl((char)strp.h_ch), strp.h_desc);
                    lower_msg = false;
                    return;
                }
            msg("unknown character '{0}'", unctrl(helpch));
            return;
        }
        /*
         * Here we print help for everything.
         * Then wait before we return to command mode
         */
        numprint = 0;
        foreach (var strp in helpstr)
            if (strp.h_print)
                numprint++;
        if ((numprint & 01) != 0)		/* round odd numbers up */
            numprint++;
        numprint /= 2;
        if (numprint > LINES - 1)
            numprint = LINES - 1;

        wclear(hw);
        cnt = 0;
        foreach (var strp in helpstr)
            if (strp.h_print)
            {
                wmove(hw, cnt % numprint, cnt >= numprint ? COLS / 2 : 0);
                if (strp.h_ch != (char)0)
                    waddstr(hw, unctrl((char)strp.h_ch));
                waddstr(hw, strp.h_desc);
                if (++cnt >= numprint * 2)
                    break;
            }
        wmove(hw, LINES - 1, 0);
        waddstr(hw, "--Press space to continue--");
        wrefresh(hw);
        wait_for(' ');
        clearok(stdscr, true);
        msg("");
        touchwin(stdscr);
        wrefresh(stdscr);
    }

    // Tell the player what a certain thing is.
    void identify()
    {
        int ch;
        h_list hp;
        string str;
        h_list[] ident_list =
        {
            new h_list('|',     "wall of a room",       false),
            new h_list('-',     "wall of a room",       false),
            new h_list(GOLD,    "gold",                 false),
            new h_list(STAIRS,  "a staircase",          false),
            new h_list(DOOR,    "door",                 false),
            new h_list(FLOOR,   "room floor",           false),
            new h_list(PLAYER,  "you",                  false),
            new h_list(PASSAGE, "passage",              false),
            new h_list(TRAP,    "trap",                 false),
            new h_list(POTION,  "potion",               false),
            new h_list(SCROLL,  "scroll",               false),
            new h_list(FOOD,    "food",                 false),
            new h_list(WEAPON,  "weapon",               false),
            new h_list(' ',     "solid rock",           false),
            new h_list(ARMOR,   "armor",                false),
            new h_list(AMULET,  "the Amulet of Yendor", false),
            new h_list(RING,    "ring",                 false),
            new h_list(STICK,   "wand or staff",        false)
        };

        msg("what do you want identified? ");
        ch = readchar();
        mpos = 0;
        if (ch == ESCAPE)
        {
            msg("");
            return;
        }
        if (Char.IsUpper((char)ch))
            str = monsters[ch-'A'].m_name;
        else
        {
            str = "unknown character";
            for (int i = 0; i < ident_list.Length; i++)
            {
                hp = ident_list[i];
                if (hp.h_ch == ch)
                {
                    str = hp.h_desc;
                    break;
                }
            }
        }
        msg("'{0}': {1}", unctrl((char)ch), str);
    }

    // He wants to go down a level
    void d_level()
    {
        if (levit_check())
            return;
        if (chat(hero.y, hero.x) != STAIRS)
            msg("I see no way down");
        else
        {
            level++;
            seenstairs = false;
            new_level();
        }
    }

    // He wants to go up a level
    void u_level()
    {
        if (levit_check())
            return;
        if (chat(hero.y, hero.x) == STAIRS)
            if (amulet)
            {
                level--;
                if (level == 0)
                    total_winner();
                new_level();
                msg("you feel a wrenching sensation in your gut");
            }
            else
                msg("your way is magically blocked");
        else
            msg("I see no way up");
    }

    // Check to see if she's levitating, and if she is, print an
    // appropriate message.
    bool levit_check()
    {
        if (!on(player, ISLEVIT))
            return false;
        msg("You can't.  You're floating off the ground!");
        return true;
    }

    // Allow a user to call a potion, scroll, or ring something
    void call()
    {
        THING obj;
        obj_info op = null;
        string guess, elsewise = null;
        bool know;
        bool isitem = false;

        obj = get_item("call", CALLABLE);
        /*
         * Make certain that it is somethings that we want to wear
         */
        if (obj == null)
            return;
        switch (obj.o_type)
        {
            case RING:
                op = ring_info[obj.o_which];
                elsewise = r_stones[obj.o_which];
                goto norm;
            case POTION:
                op = pot_info[obj.o_which];
                elsewise = p_colors[obj.o_which];
                goto norm;
            case SCROLL:
                op = scr_info[obj.o_which];
                elsewise = s_names[obj.o_which];
                goto norm;
            case STICK:
                op = ws_info[obj.o_which];
                elsewise = ws_made[obj.o_which];
            norm:
                isitem = true;
                know = op.oi_know;
                guess = op.oi_guess;
                if (!string.IsNullOrEmpty(guess))
                    elsewise = guess;
                break;
            case FOOD:
                msg("you can't call that anything");
                return;
            default:
                guess = obj.o_label;
                know = false;
                elsewise = obj.o_label;
                break;
        }
        if (know)
        {
            msg("that has already been identified");
            return;
        }
        if (!string.IsNullOrEmpty(elsewise) && elsewise == guess)
        {
            if (!terse)
                addmsg("Was ");
            msg("called \"{0}\"", elsewise);
        }
        if (terse)
            msg("call it: ");
        else
            msg("what do you want to call it? ");

        if (string.IsNullOrEmpty(elsewise))
            prbuf = "";
        else
            prbuf = elsewise;
        string buf;
        if (get_str(out buf, stdscr) == NORM)
        {
            if (isitem)
                op.oi_guess = buf;
            else
                obj.o_label = buf;
        }
    }

    // Print the current weapon/armor
    void current(THING cur, string how, string where)
    {
        after = false;
        if (cur != null)
        {
            if (!terse)
                addmsg("you are {0} (", how);
            inv_describe = false;
            addmsg("{0}) {1}", cur.o_packch, inv_name(cur, true));
            inv_describe = true;
            if (string.IsNullOrEmpty(where))
                addmsg(" {0}", where);
            endmsg();
        }
        else
        {
            if (!terse)
                addmsg("you are ");
            addmsg("{0} nothing", how);
            if (string.IsNullOrEmpty(where))
                addmsg(" {0}", where);
            endmsg();
        }
    }
}