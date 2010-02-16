/*
 * hero movement commands
 *
 * @(#)move.c	4.49 (Berkeley) 02/05/99
 *
 * Rogue: Exploring the Dungeons of Doom
 * Copyright (C) 1980-1983, 1985, 1999 Michael Toy, Ken Arnold and Glenn Wichman
 * All rights reserved.
 *
 * See the file LICENSE.TXT for full copyright and licensing information.
 */

partial class Rogue
{
    // used to hold the new hero position
    coord nh = new coord();

    // Start the hero running
    void do_run(char ch)
    {
        running = true;
        after = false;
        runch = ch;
    }

    // Check to see that a move is legal.  If it is handle the
    // consequences (fighting, picking up, etc.)
    void do_move(int dy, int dx)
    {
        char ch;
        int fl;

        firstmove = false;
        if (no_move != 0)
        {
            no_move--;
            msg("you are still stuck in the bear trap");
            return;
        }
        /*
         * Do a confused move (maybe)
         */
        bool skipOver = false;
        if (on(player, ISHUH) && rnd(5) != 0)
        {
            nh = rndmove(player);
            if (ce(nh, hero))
            {
                after = false;
                running = false;
                to_death = false;
                return;
            }
            skipOver = true;
        }

    over:
        if (!skipOver)
        {
            nh.y = hero.y + dy;
            nh.x = hero.x + dx;
        }
        skipOver = false;

        /*
         * Check if he tried to move off the screen or make an illegal
         * diagonal move, and stop him if he did.
         */
        if (nh.x < 0 || nh.x >= NUMCOLS || nh.y <= 0 || nh.y >= NUMLINES - 1)
            goto hit_bound;
        if (!diag_ok(hero, nh))
        {
            after = false;
            running = false;
            return;
        }
        if (running && ce(hero, nh))
            after = running = false;
        fl = flat(nh.y, nh.x);
        ch = winat(nh.y, nh.x);
        if (!((fl & F_REAL) != 0) && ch == FLOOR)
        {
            if (!on(player, ISLEVIT))
            {
                ch = TRAP;
                chat(nh.y, nh.x, ch);
                fl |= F_REAL;
                flat(nh.y, nh.x, fl);
            }
        }
        else if (on(player, ISHELD) && ch != 'F')
        {
            msg("you are being held");
            return;
        }
        switch (ch)
        {
            case ' ':
            case '|':
            case '-':
                goto hit_bound;
            case DOOR:
                running = false;
                if ((flat(hero.y, hero.x) & F_PASS) != 0)
                    enter_room(nh);
                goto move_stuff;
            case TRAP:
                ch = (char)be_trapped(nh);
                if (ch == T_DOOR || ch == T_TELEP)
                    return;
                goto move_stuff;
            case PASSAGE:
                /*
                 * when you're in a corridor, you don't know if you're in
                 * a maze room or not, and there ain't no way to find out
                 * if you're leaving a maze room, so it is necessary to
                 * always recalculate proom.
                 */
                proom = roomin(hero);
                goto move_stuff;
            case FLOOR:
                if ((fl & F_REAL) == 0)
                    be_trapped(hero);
                goto move_stuff;
            case STAIRS:
                seenstairs = true;
                goto default;
            default:
                running = false;
                if (char.IsUpper(ch) || moat(nh.y, nh.x) != null)
                    fight(nh, cur_weapon, false);
                else
                {
                    if (ch != STAIRS)
                        take = ch;
                    goto move_stuff;
                }
               return;
        }

    hit_bound:
        if (passgo && running && ((proom.r_flags & ISGONE) != 0)
            && !on(player, ISBLIND))
        {
            bool b1, b2;
            bool gotoOver = false;
            switch (runch)
            {
                case 'h':
                case 'l':
                    b1 = (bool)(hero.y != 1 && turn_ok(hero.y - 1, hero.x));
                    b2 = (bool)(hero.y != NUMLINES - 2 && turn_ok(hero.y + 1, hero.x));
                    if (!(b1 ^ b2))
                        break;
                    if (b1)
                    {
                        runch = 'k';
                        dy = -1;
                    }
                    else
                    {
                        runch = 'j';
                        dy = 1;
                    }
                    dx = 0;
                    turnref();
                    gotoOver = true;
                    break;
                case 'j':
                case 'k':
                    b1 = (bool)(hero.x != 0 && turn_ok(hero.y, hero.x - 1));
                    b2 = (bool)(hero.x != NUMCOLS - 1 && turn_ok(hero.y, hero.x + 1));
                    if (!(b1 ^ b2))
                        break;
                    if (b1)
                    {
                        runch = 'h';
                        dx = -1;
                    }
                    else
                    {
                        runch = 'l';
                        dx = 1;
                    }
                    dy = 0;
                    turnref();
                    gotoOver = true;
                    break;
            }
            if (gotoOver)
                goto over;
        }
        running = false;
        after = false;
        return;
    move_stuff:
        mvaddch(hero.y, hero.x, floor_at());
        if (((fl & F_PASS) != 0) && chat(oldpos.y, oldpos.x) == DOOR)
            leave_room(nh);
        hero.CopyFrom(nh);
    }

    // Decide whether it is legal to turn onto the given space
    bool turn_ok(int y, int x)
    {
        PLACE pp;

        pp = INDEX(y, x);
        return (pp.p_ch == DOOR || (pp.p_flags & (F_REAL | F_PASS)) == (F_REAL | F_PASS));
    }

    // Decide whether to refresh at a passage turning or not
    void turnref()
    {
        PLACE pp;

        pp = INDEX(hero.y, hero.x);
        if (!((pp.p_flags & F_SEEN) != 0))
        {
            if (jump)
            {
                //leaveok(stdscr, TRUE);
                refresh();
                //leaveok(stdscr, FALSE);
            }
            pp.p_flags |= F_SEEN;
        }
    }

    // Called to illuminate a room.  If it is dark, remove anything
    // that might move.
    void door_open(room rp)
    {
        int y, x;

        if (!((rp.r_flags & ISGONE) == ISGONE))
            for (y = rp.r_pos.y; y < rp.r_pos.y + rp.r_max.y; y++)
                for (x = rp.r_pos.x; x < rp.r_pos.x + rp.r_max.x; x++)
                    if (char.IsUpper(winat(y, x)))
                        wake_monster(y, x);
    }

    // The guy stepped on a trap.... Make him pay.
    int be_trapped(coord tc)
    {
        PLACE pp;
        THING arrow;
        int tr;

        if (on(player, ISLEVIT))
            return T_RUST;	/* anything that's not a door or teleport */
        running = false;
        count = 0;
        pp = INDEX(tc.y, tc.x);
        pp.p_ch = TRAP;
        tr = pp.p_flags & F_TMASK;
        pp.p_flags |= F_SEEN;
        switch (tr)
        {
            case T_DOOR:
                level++;
                new_level();
                msg("you fell into a trap!");
                break;
            case T_BEAR:
                no_move += BEARTIME;
                msg("you are caught in a bear trap");
                break;
            case T_MYST:
                switch(rnd(11))
                {
                    case 0: msg("you are suddenly in a parallel dimension"); break;
                    case  1: msg("the light in here suddenly seems %s", rainbow[rnd(cNCOLORS)]); break;
                    case  2: msg("you feel a sting in the side of your neck"); break;
                    case  3: msg("multi-colored lines swirl around you, then fade"); break;
                    case  4: msg("a %s light flashes in your eyes", rainbow[rnd(cNCOLORS)]); break;
                    case  5: msg("a spike shoots past your ear!"); break;
                    case  6: msg("%s sparks dance across your armor", rainbow[rnd(cNCOLORS)]); break;
                    case  7: msg("you suddenly feel very thirsty"); break;
                    case  8: msg("you feel time speed up suddenly"); break;
                    case  9: msg("time now seems to be going slower"); break;
                    case  10: msg("you pack turns %s!", rainbow[rnd(cNCOLORS)]); break;
                }
                break;
            case T_SLEEP:
                no_command += SLEEPTIME;
                player.t_flags &= ~ISRUN;
                msg("a strange white mist envelops you and you fall asleep");
                break;
            case T_ARROW:
                if (swing(pstats.s_lvl - 1, pstats.s_arm, 1))
                {
                   pstats.s_hpt -= roll(1, 6);
                    if (pstats.s_hpt <= 0)
                    {
                        msg("an arrow killed you");
                        death('a');
                    }
                    else
                        msg("oh no! An arrow shot you");
                }
                else
                {
                    arrow = new_item();
                    init_weapon(arrow, ARROW);
                    arrow.o_count = 1;
                    arrow.o_pos = hero;
                    fall(arrow, false);
                    msg("an arrow shoots past you");
                }
                break;
            case T_TELEP:
                /*
                 * since the hero's leaving, look() won't put a TRAP
                 * down for us, so we have to do it ourself
                 */
                teleport();
                mvaddch(tc.y, tc.x, TRAP);
                break;
            case T_DART:
                if (!swing(pstats.s_lvl+1, pstats.s_arm, 1))
                    msg("a small dart whizzes by your ear and vanishes");
                else
                {
                    pstats.s_hpt -= roll(1, 4);
                    if (pstats.s_hpt <= 0)
                    {
                        msg("a poisoned dart killed you");
                        death('d');
                    }
                    if (!ISWEARING(R_SUSTSTR) && !save(VS_POISON))
                        chg_str(-1);
                    msg("a small dart just hit you in the shoulder");
                }
                break;
            case T_RUST:
                msg("a gush of water hits you on the head");
                rust_armor(cur_armor);
                break;
            }
        //flush_type();
        return tr;
    }

    static coord ret = new coord();  /* what we will be returning */

    // Move in a random direction if the monster/person is confused
    coord rndmove(THING who)
    {
        THING obj;
        int x, y;
        char ch;

        y = ret.y = who.t_pos.y + rnd(3) - 1;
        x = ret.x = who.t_pos.x + rnd(3) - 1;
        /*
         * Now check to see if that's a legal move.  If not, don't move.
         * (I.e., bump into the wall or whatever)
         */
        if (y == who.t_pos.y && x == who.t_pos.x)
            return ret;
        if (!diag_ok(who.t_pos, ret))
            goto bad;
        else
        {
            ch = winat(y, x);
            if (!step_ok(ch))
                goto bad;
            if (ch == SCROLL)
            {
                for (obj = lvl_obj; obj != null; obj = next(obj))
                    if (y == obj.o_pos.y && x == obj.o_pos.x)
                        break;
                if (obj != null && obj.o_which == S_SCARE)
                    goto bad;
            }
        }
        return ret;

    bad:
        ret.CopyFrom(who.t_pos);
        return ret;
    }

    // Rust the given armor, if it is a legal kind to rust, and we
    // aren't wearing a magic ring.
    void rust_armor(THING arm)
    {
        if (arm == null || arm.o_type != ARMOR || arm.o_which == LEATHER ||
                arm.o_arm >= 9)
            return;

        if (((arm.o_flags & ISPROT) != 0) || ISWEARING(R_SUSTARM))
        {
            if (!to_death)
                msg("the rust vanishes instantly");
        }
        else
        {
            arm.o_arm++;
            if (!terse)
                msg("your armor appears to be weaker now. Oh my!");
            else
                msg("your armor weakens");
        }
    }
}
