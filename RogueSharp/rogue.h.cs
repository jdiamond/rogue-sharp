/*
 * Rogue definitions and variable declarations
 *
 * @(#)rogue.h	5.42 (Berkeley) 08/06/83
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
    char CCHAR(int x) 
    {
        return ((char)(x & A_CHARTEXT));
    }

    // Maximum number of different things
    private const int MAXROOMS = 9;
    private const int MAXTHINGS = 9;
    private const int MAXOBJ = 9;
    private const int MAXPACK = 23;
    private const int MAXTRAPS = 10;
    private const int AMULETLEVEL = 26;
    private const int NUMTHINGS = 7; /* number of types of things */
    private const int MAXPASS = 13; /* upper limit on number of passages */
    private const int NUMLINES = 24;
    private const int NUMCOLS = 80;
    private const int STATLINE = (NUMLINES - 1);
    private const int BORE_LEVEL = 50;

    // return values for get functions
    private const int NORM = 0; /* normal exit */
    private const int QUIT = 1; /* quit option setting */
    private const int MINUS = 2; /* back up one option */

    // inventory types
    private const int INV_OVER = 0;
    private const int INV_SLOW = 1;
    private const int INV_CLEAR = 2;

    THING next(THING ptr)
    {
        return ptr.l_next;
    }

    THING prev(THING ptr)
    {
        return ptr.l_prev;
    }

    char winat(int y, int x)
    {
        return (moat(y, x) != null ? moat(y, x).t_disguise : chat(y, x));
    }

    bool ce(coord a, coord b)
    {
        return ((a).x == (b).x && (a).y == (b).y);
    }

    coord hero
    {
        get { return player.t_pos; }
        set { player.t_pos = value; }
    }

    stats pstats
    {
        get { return player.t_stats; }
        set { player.t_stats = value; }
    }

    THING pack
    {
        get { return player.t_pack; }
        set { player.t_pack = value; }
    }

    room proom
    {
        get { return player.t_room; }
        set { player.t_room = value; }
    }

    int max_hp
    {
        get { return player.t_stats.s_maxhp; }
        set { player.t_stats.s_maxhp = value; }
    }

    bool on(THING thing, int flag)
    {
        return (thing.t_flags & flag) != 0;
    }

    int GOLDCALC()
    {
        return rnd(50 + 10 * level) + 2;
    }

    bool ISRING(int h, int r)
    {
        return (cur_ring[h] != null && cur_ring[h].o_which == r);
    }

    bool ISWEARING(int r)
    {
        return (ISRING(LEFT, r) || ISRING(RIGHT, r));
    }

    bool ISMULT(int type)
    {
        return type == POTION || type == SCROLL || type == FOOD;
    }

    PLACE INDEX(int y, int x)
    {
        return (places[((x) << 5) + (y)]);
    }

    char chat(int y, int x)
    {
        return places[(x << 5) + y].p_ch;
    }

    void chat(int y, int x, char ch)
    {
        places[(x << 5) + y].p_ch = ch;
    }

    int flat(int y, int x)
    {
        return places[(x << 5) + y].p_flags;
    }

    void flat(int y, int x, int flags)
    {
        places[(x << 5) + y].p_flags = flags;
    }

    THING moat(int y, int x)
    {
        return places[(x << 5) + y].p_monst;
    }

    void moat(int y, int x, THING monst)
    {
        places[(x << 5) + y].p_monst = monst;
    }

    // things that appear on the screens
    private const char PASSAGE = '#';
    private const char DOOR = '+';
    private const char FLOOR = '.';
    private const char PLAYER = '@';
    private const char TRAP = '^';
    private const char STAIRS = '%';
    private const char GOLD = '*';
    private const char POTION = '!';
    private const char SCROLL = '?';
    private const char MAGIC = '$';
    private const char FOOD = ':';
    private const char WEAPON = ')';
    private const char ARMOR = ']';
    private const char AMULET = ',';
    private const char RING = '=';
    private const char STICK = '/';
    private const int CALLABLE = -1;
    private const int R_OR_S = -2;

    // Various constants
    int BEARTIME { get { return spread(3); } }
    int SLEEPTIME { get { return spread(5); } }
    int WANDERTIME { get { return spread(70); } }
    int BEFORE { get { return spread(1); } }
    int AFTER { get { return spread(2); } }
    private const int HEALTIME = 30;
    private const int HUHDURATION = 20;
    private const int SEEDURATION = 850;
    private const int HUNGERTIME = 1300;
    private const int MORETIME = 150;
    private const int STOMACHSIZE = 2000;
    private const int STARVETIME = 850;
    private const int ESCAPE = 27;
    private const int LEFT = 0;
    private const int RIGHT = 1;
    private const int BOLT_LENGTH = 6;
    private const int LAMPDIST = 3;

    // Save against things
    private int VS_POISON = 00;
    private int VS_PARALYZATION = 00;
    private int VS_DEATH = 00;
    private int VS_BREATH = 02;
    private int VS_MAGIC = 03;

    // Various flag bits
    private static int ISDARK = Convert.ToInt32("0000001", 8); /* room is dark */
    private static int ISGONE = Convert.ToInt32("0000002", 8); /* room is gone (a corridor) */
    private static int ISMAZE = Convert.ToInt32("0000004", 8); /* room is gone (a corridor) */

    // flags for objects
    private static int ISCURSED = Convert.ToInt32("000001", 8); /* object is cursed */
    private static int ISKNOW = Convert.ToInt32("0000002", 8); /* player knows details about the object */
    private static int ISMISL = Convert.ToInt32("0000004", 8); /* object is a missile type */
    private static int ISMANY = Convert.ToInt32("0000010", 8); /* object comes in groups */
    private static int ISPROT = Convert.ToInt32("0000040", 8); /* armor is permanently protected */

    // flags for creatures
    private static int CANHUH = Convert.ToInt32("0000001", 8); /* creature can confuse */
    private static int CANSEE = Convert.ToInt32("0000002", 8); /* creature can see invisible creatures */
    private static int ISBLIND = Convert.ToInt32("0000004", 8); /* creature is blind */
    private static int ISCANC = Convert.ToInt32("0000010", 8); /* creature has special qualities cancelled */
    private static int ISLEVIT = Convert.ToInt32("0000010", 8); /* hero is levitating */
    private static int ISFOUND = Convert.ToInt32("0000020", 8); /* creature has been seen (used for objects) */
    private static int ISGREED = Convert.ToInt32("0000040", 8); /* creature runs to protect gold */
    private static int ISHASTE = Convert.ToInt32("0000100", 8); /* creature has been hastened */
    private static int ISTARGET = Convert.ToInt32("000200", 8); /* creature is the target of an 'f' command */
    private static int ISHELD = Convert.ToInt32("0000400", 8); /* creature has been held */
    private static int ISHUH = Convert.ToInt32("0001000", 8); /* creature is confused */
    private static int ISINVIS = Convert.ToInt32("0002000", 8); /* creature is invisible */
    private static int ISMEAN = Convert.ToInt32("0004000", 8); /* creature can wake when player enters room */
    private static int ISHALU = Convert.ToInt32("0004000", 8); /* hero is on acid trip */
    private static int ISREGEN = Convert.ToInt32("0010000", 8); /* creature can regenerate */
    private static int ISRUN = Convert.ToInt32("0020000", 8); /* creature is running at the player */
    private static int SEEMONST = Convert.ToInt32("040000", 8); /* hero can detect unseen monsters */
    private static int ISFLY = Convert.ToInt32("0040000", 8); /* creature can fly */
    private static int ISSLOW = Convert.ToInt32("0100000", 8); /* creature has been slowed */

    // Flags for level map
    private const int F_PASS = 0x80; /* is a passageway */
    private const int F_SEEN = 0x40; /* have seen this spot before */
    private const int F_DROPPED = 0x20; /* object was dropped here */
    private const int F_LOCKED = 0x20; /* door is locked */
    private const int F_REAL = 0x10; /* what you see is what you get */
    private const int F_PNUM = 0x0f; /* passage number mask */
    private const int F_TMASK = 0x07; /* trap number mask */

    // Trap types
    private const int T_DOOR = 00;
    private const int T_ARROW = 01;
    private const int T_SLEEP = 02;
    private const int T_BEAR = 03;
    private const int T_TELEP = 04;
    private const int T_DART = 05;
    private const int T_RUST = 06;
    private const int T_MYST = 07;
    private const int NTRAPS = 8;

    // Potion types
    private const int P_CONFUSE = 0;
    private const int P_LSD = 1;
    private const int P_POISON = 2;
    private const int P_STRENGTH = 3;
    private const int P_SEEINVIS = 4;
    private const int P_HEALING = 5;
    private const int P_MFIND = 6;
    private const int P_TFIND = 7;
    private const int P_RAISE = 8;
    private const int P_XHEAL = 9;
    private const int P_HASTE = 10;
    private const int P_RESTORE = 11;
    private const int P_BLIND = 12;
    private const int P_LEVIT = 13;
    private const int MAXPOTIONS = 14;

    // Scroll types
    private const int S_CONFUSE = 0;
    private const int S_MAP = 1;
    private const int S_HOLD = 2;
    private const int S_SLEEP = 3;
    private const int S_ARMOR = 4;
    private const int S_ID_POTION = 5;
    private const int S_ID_SCROLL = 6;
    private const int S_ID_WEAPON = 7;
    private const int S_ID_ARMOR = 8;
    private const int S_ID_R_OR_S = 9;
    private const int S_SCARE = 10;
    private const int S_FDET = 11;
    private const int S_TELEP = 12;
    private const int S_ENCH = 13;
    private const int S_CREATE = 14;
    private const int S_REMOVE = 15;
    private const int S_AGGR = 16;
    private const int S_PROTECT = 17;
    private const int MAXSCROLLS = 18;

    // Weapon types
    private const int MACE = 0;
    private const int SWORD = 1;
    private const int BOW = 2;
    private const int ARROW = 3;
    private const int DAGGER = 4;
    private const int TWOSWORD = 5;
    private const int DART = 6;
    private const int SHIRAKEN = 7;
    private const int SPEAR = 8;
    private const int FLAME = 9; /* fake entry for dragon breath (ick) */
    private const int MAXWEAPONS = 9; /* this should equal FLAME */

    // Armor types
    private const int LEATHER = 0;
    private const int RING_MAIL = 1;
    private const int STUDDED_LEATHER = 2;
    private const int SCALE_MAIL = 3;
    private const int CHAIN_MAIL = 4;
    private const int SPLINT_MAIL = 5;
    private const int BANDED_MAIL = 6;
    private const int PLATE_MAIL = 7;
    private const int MAXARMORS = 8;

    // Ring types
    private const int R_PROTECT = 0;
    private const int R_ADDSTR = 1;
    private const int R_SUSTSTR = 2;
    private const int R_SEARCH = 3;
    private const int R_SEEINVIS = 4;
    private const int R_NOP = 5;
    private const int R_AGGR = 6;
    private const int R_ADDHIT = 7;
    private const int R_ADDDAM = 8;
    private const int R_REGEN = 9;
    private const int R_DIGEST = 10;
    private const int R_TELEPORT = 11;
    private const int R_STEALTH = 12;
    private const int R_SUSTARM = 13;
    private const int MAXRINGS = 14;

    // Rod/Wand/Staff types
    private const int WS_LIGHT = 0;
    private const int WS_INVIS = 1;
    private const int WS_ELECT = 2;
    private const int WS_FIRE = 3;
    private const int WS_COLD = 4;
    private const int WS_POLYMORPH = 5;
    private const int WS_MISSILE = 6;
    private const int WS_HASTE_M = 7;
    private const int WS_SLOW_M = 8;
    private const int WS_DRAIN = 9;
    private const int WS_NOP = 10;
    private const int WS_TELAWAY = 11;
    private const int WS_TELTO = 12;
    private const int WS_CANCEL = 13;
    private const int MAXSTICKS = 14;

    // Now we define the structures and types

    // Help list
    class h_list
    {
        public int h_ch;
        public string h_desc;
        public bool h_print;

        public h_list(int hCh, string hDesc, bool hPrint)
        {
            h_ch = hCh;
            h_desc = hDesc;
            h_print = hPrint;
        }
    }

    // Coordinate data type
    class coord
    {
        public int x;
        public int y;

        public coord()
        {
        }

        public coord(int y, int x)
        {
            this.y = y;
            this.x = x;
        }

        public void CopyFrom(coord src)
        {
            x = src.x;
            y = src.y;
        }

        public override string ToString()
        {
            return string.Format("(y:{0},x:{1})", y, x);
        }
    }

    // Stuff about objects
    class obj_info
    {
        public string oi_name;
        public int oi_prob;
        public int oi_worth;
        public string oi_guess;
        public bool oi_know;

        public obj_info(string oiName, int oiProb, int oiWorth, string oiGuess, bool oiKnow)
        {
            oi_name = oiName;
            oi_prob = oiProb;
            oi_worth = oiWorth;
            oi_guess = oiGuess;
            oi_know = oiKnow;
        }

        public obj_info(int oiProb)
        {
            oi_prob = oiProb;
        }
    }

    // Room structure
    class room
    {
        public coord r_pos = new coord();			/* Upper left corner */
        public coord r_max = new coord();			/* Size of room */
        public coord r_gold = new coord();			/* Where the gold is */
        public int r_goldval;			/* How much the gold is worth */
        public int r_flags;			/* info about the room */
        public int r_nexits;			/* Number of exits */
        public coord[] r_exit = new coord[12];			/* Where the exits are */

        public room()
        {
            for (int i = 0; i < r_exit.Length; i++)
            {
                r_exit[i] = new coord();
            }
        }

        public room(int flags) : this()
        {
            r_flags = flags;
        }
    }

    // Structure describing a fighting being
    class stats
    {
        public int s_str;			/* Strength */
        public int s_exp;				/* Experience */
        public int s_lvl;				/* level of mastery */
        public int s_arm;				/* Armor class */
        public int s_hpt;			/* Hit points */
        public string s_dmg;			/* String describing damage done */
        public int s_maxhp;			/* Max hit points */

        public stats()
        {
        }

        public stats(int sStr, int sExp, int sLvl, int sArm, int sHpt, string sDmg)
        {
            s_str = sStr;
            s_exp = sExp;
            s_lvl = sLvl;
            s_arm = sArm;
            s_hpt = sHpt;
            s_dmg = sDmg;
        }

        public stats(int sStr, int sExp, int sLvl, int sArm, int sHpt, string sDmg, int sMaxhp)
        {
            s_str = sStr;
            s_exp = sExp;
            s_lvl = sLvl;
            s_arm = sArm;
            s_hpt = sHpt;
            s_dmg = sDmg;
            s_maxhp = sMaxhp;
        }

        public stats Copy()
        {
            return (stats)MemberwiseClone();
        }
    };

    // Structure for monsters and player
    class THING
    {
        public THING l_next; /* Next pointer in link */
        public THING l_prev;

        public coord t_pos = new coord(); /* Position */
        public bool t_turn; /* If slowed, is it a turn to move */
        public char t_type; /* What it is */
        public char t_disguise; /* What mimic looks like */
        public char t_oldch; /* Character that was where it was */
        public coord t_dest = new coord(); /* Where it is running to */
        public int t_flags; /* State word */
        public stats t_stats; /* Physical description */
        public room t_room; /* Current room for thing */
        public THING t_pack; /* What the thing is carrying */
        public int t_reserved;

        public int o_type; /* What kind of object it is */
        public coord o_pos = new coord(); /* Where it lives on the screen */
        public string o_text; /* What it says if you read it */
        public int o_launch; /* What you need to launch it */
        public char o_packch; /* What character it is in the pack */
        public string o_damage; /* Damage if used like sword */
        public string o_hurldmg; /* Damage if thrown */
        public int o_count; /* count for plural objects */
        public int o_which; /* Which object of a type it is */
        public int o_hplus; /* Plusses to hit */
        public int o_dplus; /* Plusses to damage */
        public int o_arm; /* Armor protection */
        public int o_flags; /* information about objects */
        public int o_group; /* group number for this object */
        public string o_label; /* Label for object */

        public int o_charges
        {
            get { return o_arm; }
            set { o_arm = value; }
        }

        public int o_goldval
        {
            get { return o_arm; }
            set { o_arm = value; }
        }

        public THING Copy()
        {
            var newThing = (THING)MemberwiseClone();
            newThing.t_pos = new coord();
            newThing.t_pos.CopyFrom(t_pos);
            newThing.o_pos = new coord();
            newThing.o_pos.CopyFrom(o_pos);
            return newThing;
        }
    }

    // describe a place on the level map
    class PLACE
    {
        public char p_ch;
        public int p_flags;
        public THING p_monst;
    }

    // Array containing information on all the various types of monsters
    class monster
    {
        public string m_name; /* What to call the monster */
        public int m_carry; /* Probability of carrying something */
        public int m_flags; /* things about the monster */
        public stats m_stats; /* Initial stats */

        public monster(string mName, int mCarry, int mFlags, stats mStats)
        {
            m_name = mName;
            m_carry = mCarry;
            m_flags = mFlags;
            m_stats = mStats.Copy();
        }
    };

    private const int MAXDAEMONS = 20;

    class delayed_action
    {
        public int d_type;
        public Action<int> d_func;
        public int d_arg;
        public int d_time;
    }

    class STONE 
    {
        public string st_name;
        public int st_value;

        public STONE(string name, int value)
        {
            st_name = name;
            st_value = value;
        }
    }
}
