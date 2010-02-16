/*
 * Functions for dealing with problems brought about by weapons
 *
 * @(#)weapons.c	4.34 (Berkeley) 02/05/99
 *
 * Rogue: Exploring the Dungeons of Doom
 * Copyright (C) 1980-1983, 1985, 1999 Michael Toy, Ken Arnold and Glenn Wichman
 * All rights reserved.
 *
 * See the file LICENSE.TXT for full copyright and licensing information.
 */

partial class Rogue
{
    const int NO_WEAPON = -1;
    int group = 2;

    class init_weaps
    {
        public string iw_dam;	/* Damage when wielded */
        public string iw_hrl;	/* Damage when thrown */
        public int iw_launch;	/* Launching weapon */
        public int iw_flags;	/* Miscellaneous flags */

        public init_weaps(string dam, string hrl, int launch, int flags)
        {
            iw_dam = dam;
            iw_hrl = hrl;
            iw_launch = launch;
            iw_flags = flags;
        }
    }

    init_weaps[] init_dam = new[]
    {
        new init_weaps("2x4",	"1x3",	NO_WEAPON,	0		),	/* Mace */
        new init_weaps("3x4",	"1x2",	NO_WEAPON,	0		),	/* Long sword */
        new init_weaps("1x1",	"1x1",	NO_WEAPON,	0		),	/* Bow */
        new init_weaps("1x1",	"2x3",	BOW,		ISMANY|ISMISL	),	/* Arrow */
        new init_weaps("1x6",	"1x4",	NO_WEAPON,	ISMISL|ISMISL	),	/* Dagger */
        new init_weaps("4x4",	"1x2",	NO_WEAPON,	0		),	/* 2h sword */
        new init_weaps("1x1",	"1x3",	NO_WEAPON,	ISMANY|ISMISL	),	/* Dart */
        new init_weaps("1x2",	"2x4",	NO_WEAPON,	ISMANY|ISMISL	),	/* Shuriken */
        new init_weaps("2x3",	"1x6",	NO_WEAPON,	ISMISL		),	/* Spear */
    };

    // Fire a missile in a given direction
    void missile(int ydelta, int xdelta)
    {
        THING obj;

        /*
         * Get which thing we are hurling
         */
        if ((obj = get_item("throw", WEAPON)) == null)
            return;
        if (!dropcheck(obj) || is_current(obj))
            return;
        obj = leave_pack(obj, true, false);
        do_motion(obj, ydelta, xdelta);
        /*
         * AHA! Here it has hit something.  If it is a wall or a door,
         * or if it misses (combat) the monster, put it on the floor
         */
        if (moat(obj.o_pos.y, obj.o_pos.x) == null ||
        !hit_monster(obj.o_pos.y, obj.o_pos.x, obj))
            fall(obj, true);
    }

    // Do the actual motion on the screen done by an object traveling
    // across the room
    void do_motion(THING obj, int ydelta, int xdelta)
    {
        char ch;

        /*
         * Come fly with us ...
         */
        obj.o_pos.CopyFrom(hero);
        for (; ; )
        {
            /*
             * Erase the old one
             */
            if (!ce(obj.o_pos, hero) && cansee(obj.o_pos.y, obj.o_pos.x) && !terse)
            {
                ch = chat(obj.o_pos.y, obj.o_pos.x);
                if (ch == FLOOR && !show_floor())
                    ch = ' ';
                mvaddch(obj.o_pos.y, obj.o_pos.x, ch);
            }
            /*
             * Get the new position
             */
            obj.o_pos.y += ydelta;
            obj.o_pos.x += xdelta;
            if (step_ok(ch = winat(obj.o_pos.y, obj.o_pos.x)) && ch != DOOR)
            {
                /*
                 * It hasn't hit anything yet, so display it
                 * If it alright.
                 */
                if (cansee(obj.o_pos.y, obj.o_pos.x) && !terse)
                {
                    mvaddch(obj.o_pos.y, obj.o_pos.x, obj.o_type);
                    refresh();
                }
                continue;
            }
            break;
        }
    }

    // Drop an item someplace around here.
    void fall(THING obj, bool pr)
    {
        PLACE pp;
        coord fpos = new coord();

        if (fallpos(obj.o_pos, fpos))
        {
            pp = INDEX(fpos.y, fpos.x);
            pp.p_ch = (char)obj.o_type;
            obj.o_pos.CopyFrom(fpos);
            if (cansee(fpos.y, fpos.x))
            {
                if (pp.p_monst != null)
                    pp.p_monst.t_oldch = (char)obj.o_type;
                else
                    mvaddch(fpos.y, fpos.x, obj.o_type);
            }
            attach(ref lvl_obj, obj);
            return;
        }
        if (pr)
        {
            if (has_hit)
            {
                endmsg();
                has_hit = false;
            }
            msg("the {0} vanishes as it hits the ground",
            weap_info[obj.o_which].oi_name);
        }
        discard(obj);
    }

    // Set up the initial goodies for a weapon
    void init_weapon(THING weap, int which)
    {
        init_weaps iwp;

        weap.o_type = WEAPON;
        weap.o_which = which;
        iwp = init_dam[which];
        weap.o_damage = iwp.iw_dam;
        weap.o_hurldmg = iwp.iw_hrl;
        weap.o_launch = iwp.iw_launch;
        weap.o_flags = iwp.iw_flags;
        weap.o_hplus = 0;
        weap.o_dplus = 0;
        if (which == DAGGER)
        {
            weap.o_count = rnd(4) + 2;
            weap.o_group = group++;
        }
        else if ((weap.o_flags & ISMANY) == ISMANY)
        {
            weap.o_count = rnd(8) + 8;
            weap.o_group = group++;
        }
        else
        {
            weap.o_count = 1;
            weap.o_group = 0;
        }
    }

    // Does the missile hit the monster?
    bool hit_monster(int y, int x, THING obj)
    {
        coord mp = new coord();

        mp.y = y;
        mp.x = x;
        return fight(mp, obj, true);
    }

    static string numbuf;

    // Figure out the plus number for armor/weapons
    string num(int n1, int n2, char type)
    {
        numbuf = string.Format(n1 < 0 ? "{0}" : "+{0}", n1);
        if (type == WEAPON)
            numbuf += string.Format(n2 < 0 ? ",{0}" : ",+{0}", n2);
        return numbuf;
    }

    // Pull out a certain weapon
    void wield()
    {
        THING obj, oweapon;
        string sp;

        oweapon = cur_weapon;

        if (!dropcheck(cur_weapon))
        {
            cur_weapon = oweapon;
            return;
        }

        cur_weapon = oweapon;

        if ((obj = get_item("wield", WEAPON)) == null)
        {
            after = false;
            return;
        }

        if (obj.o_type == ARMOR)
        {
            msg("you can't wield armor");
            after = false;
            return;
        }

        if (is_current(obj))
        {
            after = false;
            return;
        }

        sp = inv_name(obj, true);
        cur_weapon = obj;
        if (!terse)
            addmsg("you are now ");
        msg("wielding {0} ({1})", sp, obj.o_packch);
    }

    // Pick a random position around the give (y, x) coordinates
    bool fallpos(coord pos, coord newpos)
    {
        int y, x, cnt, ch;

        cnt = 0;
        for (y = pos.y - 1; y <= pos.y + 1; y++)
            for (x = pos.x - 1; x <= pos.x + 1; x++)
            {
                /*
                 * check to make certain the spot is empty, if it is,
                 * put the object there, set it in the level list
                 * and re-draw the room if he can see it
                 */
                if (y == hero.y && x == hero.x)
                    continue;
                if (((ch = chat(y, x)) == FLOOR || ch == PASSAGE)
                            && rnd(++cnt) == 0)
                {
                    newpos.y = y;
                    newpos.x = x;
                }
            }
        return (bool)(cnt != 0);
    }
}
