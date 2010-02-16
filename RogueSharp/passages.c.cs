/*
 * Draw the connecting passages
 *
 * @(#)passages.c	4.22 (Berkeley) 02/05/99
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
    class rdes
    {
        public bool[] conn; /* possible to connect to room i? */
        public bool[] isconn; /* connection been made to room i? */
        public bool ingraph; /* this room in graph already? */

        public rdes(bool[] conn, bool[] isconn, bool ingraph)
        {
            this.conn = conn;
            this.isconn = isconn;
            this.ingraph = ingraph;
        }
    }

    // Draw all the passages on a level.
    void do_passages()
    {
        rdes r1, r2 = null;
        int r1num, r2num = 0;
        int i, j;
        int roomcount;
        rdes[] rdes = new[] {
            new rdes( new[] { false,  true, false,  true, false, false, false, false, false }, new[] { false, false, false, false, false, false, false, false, false }, false ),
            new rdes( new[] {  true, false,  true, false,  true, false, false, false, false }, new[] { false, false, false, false, false, false, false, false, false }, false ),
            new rdes( new[] { false,  true, false, false, false,  true, false, false, false }, new[] { false, false, false, false, false, false, false, false, false }, false ),
            new rdes( new[] {  true, false, false, false,  true, false,  true, false, false }, new[] { false, false, false, false, false, false, false, false, false }, false ),
            new rdes( new[] { false,  true, false,  true, false,  true, false,  true, false }, new[] { false, false, false, false, false, false, false, false, false }, false ),
            new rdes( new[] { false, false,  true, false,  true, false, false, false,  true }, new[] { false, false, false, false, false, false, false, false, false }, false ),
            new rdes( new[] { false, false, false,  true, false, false, false,  true, false }, new[] { false, false, false, false, false, false, false, false, false }, false ),
            new rdes( new[] { false, false, false, false,  true, false,  true, false,  true }, new[] { false, false, false, false, false, false, false, false, false }, false ),
            new rdes( new[] { false, false, false, false, false,  true, false,  true, false }, new[] { false, false, false, false, false, false, false, false, false }, false ),
        };

        /*
         * starting with one room, connect it to a random adjacent room and
         * then pick a new room to start with.
         */
        roomcount = 1;
        r1num = rnd(MAXROOMS);
        r1 = rdes[r1num];
        r1.ingraph = true;
        do
        {
            /*
             * find a room to connect with
             */
            j = 0;
            for (i = 0; i < MAXROOMS; i++)
                if (r1.conn[i] && !rdes[i].ingraph && rnd(++j) == 0)
                {
                    r2num = i;
                    r2 = rdes[i];
                }
            /*
             * if no adjacent rooms are outside the graph, pick a new room
             * to look from
             */
            if (j == 0)
            {
                do
                {
                    r1num = rnd(MAXROOMS);
                    r1 = rdes[r1num];
                } while (!r1.ingraph);
            }
            /*
             * otherwise, connect new room to the graph, and draw a tunnel
             * to it
             */
            else
            {
                r2.ingraph = true;
                i = r1num;
                j = r2num;
                conn(i, j);
                r1.isconn[j] = true;
                r2.isconn[i] = true;
                roomcount++;
            }
        } while (roomcount < MAXROOMS);

        /*
         * attempt to add passages to the graph a random number of times so
         * that there isn't always just one unique passage through it.
         */
        for (roomcount = rnd(5); roomcount > 0; roomcount--)
        {
            r1num = rnd(MAXROOMS);
            r1 = rdes[r1num];	/* a random room to look from */
            /*
             * find an adjacent room not already connected
             */
            j = 0;
            for (i = 0; i < MAXROOMS; i++)
                if (r1.conn[i] && !r1.isconn[i] && rnd(++j) == 0)
                {
                    r2num = i;
                    r2 = rdes[i];
                }
            /*
             * if there is one, connect it and look for the next added
             * passage
             */
            if (j != 0)
            {
                i = r1num;
                j = r2num;
                conn(i, j);
                r1.isconn[j] = true;
                r2.isconn[i] = true;
            }
        }
        passnum();
    }

    coord del = new coord();
    coord curr = new coord();
    coord turn_delta = new coord();
    coord spos = new coord();
    coord epos = new coord();

    // Draw a corridor from a room in a certain direction.
    void conn(int r1, int r2)
    {
        room rpf, rpt = null;
        int rmt;
        int distance = 0, turn_spot, turn_distance = 0;
        int rm;
        char direc;

        if (r1 < r2)
        {
            rm = r1;
            if (r1 + 1 == r2)
                direc = 'r';
            else
                direc = 'd';
        }
        else
        {
            rm = r2;
            if (r2 + 1 == r1)
                direc = 'r';
            else
                direc = 'd';
        }
        rpf = rooms[rm];
        /*
         * Set up the movement variables, in two cases:
         * first drawing one down.
         */
        if (direc == 'd')
        {
            rmt = rm + 3;				/* room # of dest */
            rpt = rooms[rmt];			/* room pointer of dest */
            del.x = 0;				/* direction of move */
            del.y = 1;
            spos.x = rpf.r_pos.x;			/* start of move */
            spos.y = rpf.r_pos.y;
            epos.x = rpt.r_pos.x;			/* end of move */
            epos.y = rpt.r_pos.y;
            if (!((rpf.r_flags & ISGONE) == ISGONE))		/* if not gone pick door pos */
                do
                {
                    spos.x = rpf.r_pos.x + rnd(rpf.r_max.x - 2) + 1;
                    spos.y = rpf.r_pos.y + rpf.r_max.y - 1;
                } while (((rpf.r_flags & ISMAZE) == ISMAZE) && !((flat(spos.y, spos.x) & F_PASS) == F_PASS));
            if (!((rpt.r_flags & ISGONE) == ISGONE))
                do
                {
                    epos.x = rpt.r_pos.x + rnd(rpt.r_max.x - 2) + 1;
                } while (((rpt.r_flags & ISMAZE) == ISMAZE) && !((flat(epos.y, epos.x) & F_PASS) == F_PASS));
            distance = Math.Abs(spos.y - epos.y) - 1;	/* distance to move */
            turn_delta.y = 0;			/* direction to turn */
            turn_delta.x = (spos.x < epos.x ? 1 : -1);
            turn_distance = Math.Abs(spos.x - epos.x);	/* how far to turn */
        }
        else if (direc == 'r')			/* setup for moving right */
        {
            rmt = rm + 1;
            rpt = rooms[rmt];
            del.x = 1;
            del.y = 0;
            spos.x = rpf.r_pos.x;
            spos.y = rpf.r_pos.y;
            epos.x = rpt.r_pos.x;
            epos.y = rpt.r_pos.y;
            if (!((rpf.r_flags & ISGONE) == ISGONE))
                do
                {
                    spos.x = rpf.r_pos.x + rpf.r_max.x - 1;
                    spos.y = rpf.r_pos.y + rnd(rpf.r_max.y - 2) + 1;
                } while (((rpf.r_flags & ISMAZE) == ISMAZE) && !((flat(spos.y, spos.x) & F_PASS) == F_PASS));
            if (!((rpt.r_flags & ISGONE) == ISGONE))
                do
                {
                    epos.y = rpt.r_pos.y + rnd(rpt.r_max.y - 2) + 1;
                } while (((rpt.r_flags & ISMAZE) == ISMAZE) && !((flat(epos.y, epos.x) & F_PASS) == F_PASS));
            distance = Math.Abs(spos.x - epos.x) - 1;
            turn_delta.y = (spos.y < epos.y ? 1 : -1);
            turn_delta.x = 0;
            turn_distance = Math.Abs(spos.y - epos.y);
        }

        turn_spot = rnd(distance - 1) + 1;		/* where turn starts */

        /*
         * Draw in the doors on either side of the passage or just put #'s
         * if the rooms are gone.
         */
        if (!((rpf.r_flags & ISGONE) == ISGONE))
            door(rpf, spos);
        else
            putpass(spos);
        if (!((rpt.r_flags & ISGONE) == ISGONE))
            door(rpt, epos);
        else
            putpass(epos);
        /*
         * Get ready to move...
         */
        curr.x = spos.x;
        curr.y = spos.y;
        while (distance > 0)
        {
            /*
             * Move to new position
             */
            curr.x += del.x;
            curr.y += del.y;
            /*
             * Check if we are at the turn place, if so do the turn
             */
            if (distance == turn_spot)
                while (turn_distance-- > 0)
                {
                    putpass(curr);
                    curr.x += turn_delta.x;
                    curr.y += turn_delta.y;
                }
            /*
             * Continue digging along
             */
            putpass(curr);
            distance--;
        }
        curr.x += del.x;
        curr.y += del.y;
        if (!ce(curr, epos))
            msg("warning, connectivity problem on this level");
    }

    // add a passage character or secret passage here
    void putpass(coord cp)
    {
        PLACE pp;

        pp = INDEX(cp.y, cp.x);
        pp.p_flags |= F_PASS;
        if (rnd(10) + 1 < level && rnd(40) == 0)
            pp.p_flags &= ~F_REAL;
        else
            pp.p_ch = PASSAGE;
    }

    // Add a door or possibly a secret door.  Also enters the door in
    // the exits array of the room.
    void door(room rm, coord cp)
    {
        PLACE pp;

        rm.r_exit[rm.r_nexits++].CopyFrom(cp);

        if ((rm.r_flags & ISMAZE) == ISMAZE)
            return;

        pp = INDEX(cp.y, cp.x);
        if (rnd(10) + 1 < level && rnd(5) == 0)
        {
            if (cp.y == rm.r_pos.y || cp.y == rm.r_pos.y + rm.r_max.y - 1)
                pp.p_ch = '-';
            else
                pp.p_ch = '|';
            pp.p_flags &= ~F_REAL;
        }
        else
            pp.p_ch = DOOR;
    }

    static int pnum;
    static bool newpnum;

    // Assign a number to each passageway
    void passnum()
    {
        int i;

        pnum = 0;
        newpnum = false;
        foreach (var rp in passages)
            rp.r_nexits = 0;
        foreach (var rp in rooms)
            for (i = 0; i < rp.r_nexits; i++)
            {
                newpnum = true;
                numpass(rp.r_exit[i].y, rp.r_exit[i].x);
            }
    }

    // Number a passageway square and its brethren
    void numpass(int y, int x)
    {
        int fp;
        room rp;
        char ch;

        if (x >= NUMCOLS || x < 0 || y >= NUMLINES || y <= 0)
            return;
        fp = flat(y, x);
        if ((fp & F_PNUM) != 0)
            return;
        if (newpnum)
        {
            pnum++;
            newpnum = false;
        }
        /*
         * check to see if it is a door or secret door, i.e., a new exit,
         * or a numerable type of place
         */
        if ((ch = chat(y, x)) == DOOR ||
            (!((fp & F_REAL) == F_REAL) && (ch == '|' || ch == '-')))
        {
            rp = passages[pnum];
            rp.r_exit[rp.r_nexits].y = y;
            rp.r_exit[rp.r_nexits++].x = x;
        }
        else if (!((fp & F_PASS) == F_PASS))
            return;
        fp |= pnum;
        flat(y, x, fp);
        /*
         * recurse on the surrounding places
         */
        numpass(y + 1, x);
        numpass(y - 1, x);
        numpass(y, x + 1);
        numpass(y, x - 1);
    }
}
