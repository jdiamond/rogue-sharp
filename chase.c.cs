/*
 * Code for one creature to chase another
 *
 * @(#)chase.c	4.57 (Berkeley) 02/05/99
 *
 * Rogue: Exploring the Dungeons of Doom
 * Copyright (C) 1980-1983, 1985, 1999 Michael Toy, Ken Arnold and Glenn Wichman
 * All rights reserved.
 *
 * See the file LICENSE.TXT for full copyright and licensing information.
 */

using System;
using System.Diagnostics;

partial class Rogue
{
    int DRAGONSHOT = 5; // one chance in DRAGONSHOT that a dragon will flame
    coord ch_ret = new coord(); // Where chasing takes you

    // Make all the running monsters move.
    void runners(int ignored)
    {
        THING tp;
        THING nextp;
        bool wastarget;
        coord orig_pos;

        for (tp = mlist; tp != null; tp = nextp)
        {
            /* remember this in case the monster's "next" is changed */
            nextp = next(tp);
            if (!on(tp, ISHELD) && on(tp, ISRUN))
            {
                orig_pos = tp.t_pos;
                wastarget = on(tp, ISTARGET);
                if (move_monst(tp) == -1)
                        continue;
                if (on(tp, ISFLY) && dist_cp(hero, tp.t_pos) >= 3)
                    move_monst(tp);
                if (wastarget && !ce(orig_pos, tp.t_pos))
                {
                    tp.t_flags &= ~ISTARGET;
                    to_death = false;
                }
            }
        }

        if (has_hit)
        {
            endmsg();
            has_hit = false;
        }
    }

    // Execute a single turn of running for a monster
    int move_monst(THING tp)
    {
        if (!on(tp, ISSLOW) || tp.t_turn)
            if (do_chase(tp) == -1)
                return (-1);
        if (on(tp, ISHASTE))
            if (do_chase(tp) == -1)
                return (-1);
        tp.t_turn ^= true;
        return (0);
    }

    // Make the monster's new location be the specified one, updating
    // all the relevant state.
    void relocate(THING th, coord new_loc)
    {
        room oroom;

        char symbol = th.t_disguise;

        if (!ce(new_loc, th.t_pos))
        {
            Debug.WriteLine(string.Format("{0} moved to {1}.", symbol, new_loc));

            mvaddch(th.t_pos.y, th.t_pos.x, th.t_oldch);
            th.t_room = roomin(new_loc);
            set_oldch(th, new_loc);
            oroom = th.t_room;
            moat(th.t_pos.y, th.t_pos.x, null);

            if (oroom != th.t_room)
            {
                Debug.WriteLine(string.Format("{0} moved into room {1}.", symbol, oroom));
                th.t_dest = find_dest(th);
            }

            th.t_pos.CopyFrom(new_loc);
            moat(new_loc.y, new_loc.x, th);
        }

        move(new_loc.y, new_loc.x);

        if (see_monst(th))
            addch(th.t_disguise);
        else if (on(player, SEEMONST))
        {
            standout();
            addch(th.t_type);
            standend();
        }
    }

    static coord thisCoord = new coord();			/* Temporary destination for chaser */

    // Make one thing chase another.
    int do_chase(THING th)
    {
        coord cp;
        room rer, ree;	/* room of chaser, room of chasee */
        int mindist = 32767, curdist;
        bool stoprun = false;	/* TRUE means we are there */
        bool door;
        THING obj;

        rer = th.t_room;		/* Find room of chaser */
        if (on(th, ISGREED) && rer.r_goldval == 0)
            th.t_dest = hero;	/* If gold has been taken, run after hero */
        if (th.t_dest == hero)	/* Find room of chasee */
            ree = proom;
        else
            ree = roomin(th.t_dest);
        /*
         * We don't count doors as inside rooms for this routine
         */
        door = (chat(th.t_pos.y, th.t_pos.x) == DOOR);
        /*
         * If the object of our desire is in a different room,
         * and we are not in a corridor, run to the door nearest to
         * our goal.
         */
    over:
        if (rer != ree)
        {
            for (int i = 0; i < rer.r_nexits; i++)
            {
                cp = rer.r_exit[i];
                curdist = dist_cp(th.t_dest, cp);
                if (curdist < mindist)
                {
                    thisCoord.CopyFrom(cp);
                    mindist = curdist;
                }
            }
            if (door)
            {
                rer = passages[flat(th.t_pos.y, th.t_pos.x) & F_PNUM];
                door = false;
                goto over;
            }
        }
        else
        {
            thisCoord.CopyFrom(th.t_dest);
            /*
             * For dragons check and see if (a) the hero is on a straight
             * line from it, and (b) that it is within shooting distance,
             * but outside of striking range.
             */
            if (th.t_type == 'D' && (th.t_pos.y == hero.y || th.t_pos.x == hero.x
                || Math.Abs(th.t_pos.y - hero.y) == Math.Abs(th.t_pos.x - hero.x))
                && dist_cp(th.t_pos, hero) <= BOLT_LENGTH * BOLT_LENGTH
                && !on(th, ISCANC) && rnd(DRAGONSHOT) == 0)
            {
                delta.y = Math.Sign(hero.y - th.t_pos.y);
                delta.x = Math.Sign(hero.x - th.t_pos.x);
                if (has_hit)
                    endmsg();
                fire_bolt(th.t_pos, delta, "flame");
                running = false;
                count = 0;
                quiet = 0;
                if (to_death && !on(th, ISTARGET))
                {
                    to_death = false;
                    kamikaze = false;
                }
                return(0);
            }
        }
        /*
         * This now contains what we want to run to this time
         * so we run to it.  If we hit it we either want to fight it
         * or stop running
         */
        if (!chase(th, thisCoord))
        {
            if (ce(thisCoord, hero))
            {
                return( attack(th) );
            }
            else if (ce(thisCoord, th.t_dest))
            {
                for (obj = lvl_obj; obj != null; obj = next(obj))
                    if (th.t_dest == obj.o_pos)
                    {
                        detach(ref lvl_obj, obj);
                        attach(ref th.t_pack, obj);
                        chat(obj.o_pos.y, obj.o_pos.x, (th.t_room.r_flags & ISGONE) != 0 ? PASSAGE : FLOOR);
                        th.t_dest = find_dest(th);
                        break;
                    }
                if (th.t_type != 'F')
                    stoprun = true;
            }
        }
        else
        {
            if (th.t_type == 'F')
                return(0);
        }
        relocate(th, ch_ret);
        /*
         * And stop running if need be
         */
        if (stoprun && ce(th.t_pos, th.t_dest))
            th.t_flags &= ~ISRUN;
        return(0);
    }

    // Set the oldch character for the monster
    void set_oldch(THING tp, coord cp)
    {
        char sch;

        if (ce(tp.t_pos, cp))
            return;

        sch = tp.t_oldch;
        tp.t_oldch = CCHAR(mvinch(cp.y, cp.x));
        if (!on(player, ISBLIND))
        {
            if ((sch == FLOOR || tp.t_oldch == FLOOR) && ((tp.t_room.r_flags & ISDARK) != 0))
                tp.t_oldch = ' ';
            else if (dist_cp(cp, hero) <= LAMPDIST && see_floor)
                tp.t_oldch = chat(cp.y, cp.x);
        }
    }

    // Return TRUE if the hero can see the monster
    bool see_monst(THING mp)
    {
        int y, x;

        if (on(player, ISBLIND))
            return false;
        if (on(mp, ISINVIS) && !on(player, CANSEE))
            return false;
        y = mp.t_pos.y;
        x = mp.t_pos.x;
        if (dist(y, x, hero.y, hero.x) < LAMPDIST)
        {
            if (y != hero.y && x != hero.x && !step_ok(chat(y, hero.x)) && !step_ok(chat(hero.y, x)))
                return false;
            return true;
        }
        if (mp.t_room != proom)
            return false;
        return ((bool)!((mp.t_room.r_flags & ISDARK) == ISDARK));
    }

    // Set a monster running after the hero.
    void runto(coord runner)
    {
        THING tp;

        /*
         * If we couldn't find him, something is funny
         */
        tp = moat(runner.y, runner.x);
        /*
         * Start the beastie running
         */
        tp.t_flags |= ISRUN;
        tp.t_flags &= ~ISHELD;
        tp.t_dest = find_dest(tp);
    }

    static coord tryp = new coord();

    // Find the spot for the chaser(er) to move closer to the
    // chasee(ee).  Returns TRUE if we want to keep on chasing later
    // FALSE if we reach the goal.
    bool chase(THING tp, coord ee)
    {
        THING obj;
        int x, y;
        int curdist, thisdist;
        coord er = tp.t_pos;
        char ch;
        int plcnt = 1;

        /*
         * If the thing is confused, let it move randomly. Invisible
         * Stalkers are slightly confused all of the time, and bats are
         * quite confused all the time
         */
        if ((on(tp, ISHUH) && rnd(5) != 0) || (tp.t_type == 'P' && rnd(5) == 0) || (tp.t_type == 'B' && rnd(2) == 0))
        {
            /*
             * get a valid random move
             */
            ch_ret.CopyFrom(rndmove(tp));
            curdist = dist_cp(ch_ret, ee);
            /*
             * Small chance that it will become un-confused 
             */
            if (rnd(20) == 0)
                tp.t_flags &= ~ISHUH;
        }
        /*
         * Otherwise, find the empty spot next to the chaser that is
         * closest to the chasee.
         */
        else
        {
            int ey, ex;
            /*
             * This will eventually hold where we move to get closer
             * If we can't find an empty spot, we stay where we are.
             */
            curdist = dist_cp(er, ee);
            ch_ret.CopyFrom(er);

            ey = er.y + 1;
            if (ey >= NUMLINES - 1)
                ey = NUMLINES - 2;
            ex = er.x + 1;
            if (ex >= NUMCOLS)
                ex = NUMCOLS - 1;

            for (x = er.x - 1; x <= ex; x++)
            {
                if (x < 0)
                    continue;
                tryp.x = x;
                for (y = er.y - 1; y <= ey; y++)
                {
                    tryp.y = y;
                    if (!diag_ok(er, tryp))
                        continue;
                    ch = winat(y, x);
                    if (step_ok(ch))
                    {
                        /*
                         * If it is a scroll, it might be a scare monster scroll
                         * so we need to look it up to see what type it is.
                         */
                        if (ch == SCROLL)
                        {
                            for (obj = lvl_obj; obj != null; obj = next(obj))
                            {
                                if (y == obj.o_pos.y && x == obj.o_pos.x)
                                    break;
                            }
                            if (obj != null && obj.o_which == S_SCARE)
                                continue;
                        }
                        /*
                         * It can also be a Xeroc, which we shouldn't step on
                         */
                        if ((obj = moat(y, x)) != null && obj.t_type == 'X')
                            continue;
                        /*
                         * If we didn't find any scrolls at this place or it
                         * wasn't a scare scroll, then this place counts
                         */
                        thisdist = dist(y, x, ee.y, ee.x);
                        if (thisdist < curdist)
                        {
                            plcnt = 1;
                            ch_ret.CopyFrom(tryp);
                            curdist = thisdist;
                        }
                        else if (thisdist == curdist && rnd(++plcnt) == 0)
                        {
                            ch_ret.CopyFrom(tryp);
                            curdist = thisdist;
                        }
                    }
                }
            }
        }
        return (bool)(curdist != 0 && !ce(ch_ret, hero));
    }

    // Find what room some coordinates are in. NULL means they aren't
    // in any room.
    room roomin(coord cp)
    {
        int fp;

        fp = flat(cp.y, cp.x);
        if ((fp & F_PASS) != 0)
            return passages[fp & F_PNUM];

        foreach (var rp in rooms)
            if (cp.x <= rp.r_pos.x + rp.r_max.x && rp.r_pos.x <= cp.x
                && cp.y <= rp.r_pos.y + rp.r_max.y && rp.r_pos.y <= cp.y)
                return rp;

        msg("in some bizarre place ({0}, {1})", cp.y, cp.x);
        return null;
    }

    // Check to see if the move is legal if it is diagonal
    bool diag_ok(coord sp, coord ep)
    {
        if (ep.x < 0 || ep.x >= NUMCOLS || ep.y <= 0 || ep.y >= NUMLINES - 1)
            return false;
        if (ep.x == sp.x || ep.y == sp.y)
            return true;
        return (bool)(step_ok(chat(ep.y, sp.x)) && step_ok(chat(sp.y, ep.x)));
    }

    // Returns true if the hero can see a certain coordinate.
    bool cansee(int y, int x)
    {
        room rer;
        coord tp = new coord();

        if (on(player, ISBLIND))
        return false;
        if (dist(y, x, hero.y, hero.x) < LAMPDIST)
        {
            if ((flat(y, x) & F_PASS) != 0)
                if (y != hero.y && x != hero.x && !step_ok(chat(y, hero.x)) && !step_ok(chat(hero.y, x)))
                    return false;
            return true;
        }
        /*
         * We can only see if the hero in the same room as
         * the coordinate and the room is lit or if it is close.
         */
        tp.y = y;
        tp.x = x;
        return (bool)((rer = roomin(tp)) == proom && !((rer.r_flags & ISDARK) != 0));
    }

    // find the proper destination for the monster
    coord find_dest(THING tp)
    {
        THING obj;
        int prob;

        if ((prob = monsters[tp.t_type - 'A'].m_carry) <= 0 || tp.t_room == proom || see_monst(tp))
            return hero;
        for (obj = lvl_obj; obj != null; obj = next(obj))
        {
            if (obj.o_type == SCROLL && obj.o_which == S_SCARE)
                continue;
            if (roomin(obj.o_pos) == tp.t_room && rnd(100) < prob)
            {
                for (tp = mlist; tp != null; tp = next(tp))
                    if (tp.t_dest.Equals(obj.o_pos))
                        break;
                if (tp == null)
                    return obj.o_pos;
            }
        }
        return hero;
    }

    // Calculate the "distance" between to points.  Actually,
    // this calculates d^2, not d, but that's good enough for
    // our purposes, since it's only used comparitively.
    int dist(int y1, int x1, int y2, int x2)
    {
        return ((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
    }

    // Call dist() with appropriate arguments for coord pointers
    int dist_cp(coord c1, coord c2)
    {
        return dist(c1.y, c1.x, c2.y, c2.x);
    }
}
