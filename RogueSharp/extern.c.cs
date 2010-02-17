/*
 * global variable initializaton
 *
 * @(#)extern.c	4.82 (Berkeley) 02/05/99
 *
 * Rogue: Exploring the Dungeons of Doom
 * Copyright (C) 1980-1983, 1985, 1999 Michael Toy, Ken Arnold and Glenn Wichman
 * All rights reserved.
 *
 * See the file LICENSE.TXT for full copyright and licensing information.
 */

using System;
using System.Collections.Generic;

partial class Rogue
{
    bool after;				/* True if we want after daemons */
    bool again;				/* Repeating the last command */
    int noscore;				/* Was a wizard sometime */
    bool seenstairs;			/* Have seen the stairs (for lsd) */
    bool amulet = false;			/* He found the amulet */
    bool door_stop = false;			/* Stop running when we pass a door */
    bool fight_flush = false;		/* True if toilet input */
    bool firstmove = false;			/* First move after setting door_stop */
    bool has_hit = false;			/* Has a "hit" message pending in msg */
    bool inv_describe = true;		/* Say which way items are being used */
    bool jump = false;			/* Show running as series of jumps */
    bool kamikaze = false;			/* to_death really to DEATH */
    bool lower_msg = false;			/* Messages should start w/lower case */
    bool move_on = false;			/* Next move shouldn't pick up items */
    bool msg_esc = false;			/* Check for ESC from msg's --More-- */
    bool passgo = true;			/* Follow passages */
    bool playing = true;			/* True until he quits */
    bool q_comm = false;			/* Are we executing a 'Q' command? */
    bool running = false;			/* True if player is running */
    bool save_msg = true;			/* Remember last msg */
    bool see_floor = true; /* Show the lamp illuminated floor */
    bool stat_msg = false;			/* Should status() print as a msg() */
    bool terse = false; /* True if we should be short */
    bool to_death = false;			/* Fighting is to the death! */
    bool wizard = true;			/* True if allows wizard commands */

    private bool[] pack_used = new bool[26]; /* Is the character used in the pack? */

    char dir_ch;				/* Direction from last get_dir() call */
    string huh;			/* The last message printed */
    string[] p_colors = new string[MAXPOTIONS];		/* Colors of the potions */
    string prbuf;			/* buffer for sprintfs */
    string[] r_stones = new string[MAXRINGS];		/* Stone settings of the rings */
    char runch;				/* Direction player is running */
    string[] s_names = new string[MAXSCROLLS];		/* Names of the scrolls */
    char take;				/* Thing she is taking */
    string[] ws_made = new string[MAXSTICKS];		/* What sticks are made of */
    string[] ws_type = new string[MAXSTICKS];		/* Is it a wand or a staff */
    private string fruit = "slime-mold";
    char l_last_comm = '\0';		/* Last last_comm */
    char l_last_dir = '\0';			/* Last last_dir */
    char last_comm = '\0';			/* Last command typed */
    char last_dir = '\0';			/* Last direction given */
    string[] tr_name = {			/* Names of the traps */
        "a trapdoor",
        "an arrow trap",
        "a sleeping gas trap",
        "a beartrap",
        "a teleport trap",
        "a poison dart trap",
        "a rust trap",
            "a mysterious trap"
    };
    int n_objs;				/* # items listed in inventory() call */
    int ntraps;				/* Number of traps on this level */
    int hungry_state = 0;			/* How hungry is he */
    int inpack = 0; /* Number of things in pack */
    int inv_type = 0;			/* Type of inventory to use */
    public int level = 1;				/* What level she is on */
    int max_hit;				/* Max damage done to her in to_death */
    int max_level;				/* Deepest player has gone */
    int mpos = 0;				/* Where cursor is on top line */
    int no_food = 0;			/* Number of levels without food */
    int[] a_class = new[] {		/* Armor class for each armor type */
        8,	/* LEATHER */
        7,	/* RING_MAIL */
        7,	/* STUDDED_LEATHER */
        6,	/* SCALE_MAIL */
        5,	/* CHAIN_MAIL */
        4,	/* SPLINT_MAIL */
        4,	/* BANDED_MAIL */
        3,	/* PLATE_MAIL */
    };

    int count = 0;				/* Number of times to repeat command */
    int food_left;				/* Amount of food in hero's stomach */
    int lastscore = -1;			/* Score before this turn */
    int no_command = 0;			/* Number of turns asleep */
    int no_move = 0;			/* Number of turns held in place */
    int purse = 0;				/* How much gold he has */
    int quiet = 0;				/* Number of quiet turns */
    int vf_hit = 0;				/* Number of time flytrap has hit */

    public int seed;				/* Random number seed */

    private int[] e_levels = {
                                 10,
                                 20,
                                 40,
                                 80,
                                 160,
                                 320,
                                 640,
                                 1300,
                                 2600,
                                 5200,
                                 13000,
                                 26000,
                                 50000,
                                 100000,
                                 200000,
                                 400000,
                                 800000,
                                 2000000,
                                 4000000,
                                 8000000,
                                 0
                             };

    coord delta = new coord();				/* Change indicated to get_dir() */
    coord oldpos;				/* Position before last look() call */
    coord stairs;				/* Location of staircase */
    PLACE[] places = new PLACE[MAXLINES * MAXCOLS];		/* level map */
    THING cur_armor;			/* What he is wearing */
    THING[] cur_ring = new THING[2];			/* Which rings are being worn */
    THING cur_weapon;			/* Which weapon he is weilding */
    THING l_last_pick = null;		/* Last last_pick */
    THING last_pick = null;		/* Last object picked in get_item() */
    THING lvl_obj = null;			/* List of objects on this level */
    THING mlist = null;			/* List of monsters on the level */

    public IEnumerable<THING> MonstersOnLevel
    {
        get
        {
            for (var mp = mlist; mp != null; mp = next(mp))
                yield return mp;
        }
    }

    THING player = new THING();				/* His stats */

    IntPtr hw = IntPtr.Zero;			/* used as a scratch window */

    private stats max_stats = new stats(16, 0, 1, 10, 12, "1x4", 12);

    room oldrp;			/* Roomin(&oldpos) */
    public room[] rooms = new room[MAXROOMS];		/* One for each room -- A level */
    room[] passages = new room[MAXPASS] 		/* One for each passage */
    {
        new room(ISGONE|ISDARK),
        new room(ISGONE|ISDARK),
        new room(ISGONE|ISDARK),
        new room(ISGONE|ISDARK),
        new room(ISGONE|ISDARK),
        new room(ISGONE|ISDARK),
        new room(ISGONE|ISDARK),
        new room(ISGONE|ISDARK),
        new room(ISGONE|ISDARK),
        new room(ISGONE|ISDARK),
        new room(ISGONE|ISDARK),
        new room(ISGONE|ISDARK),
        new room(ISGONE|ISDARK),
    };

    private monster[] monsters = new[]
                                     {
                                         /* Name, CARRY, FLAG, str, exp, lvl, amr, hpt, dmg */
                                         new monster("aquator", 0, ISMEAN, new stats(10, 20, 5, 2, 1, "0x0/0x0")),
                                         new monster("bat", 0, ISFLY, new stats(10, 1, 1, 3, 1, "1x2")),
                                         new monster("centaur", 15, 0, new stats(10, 17, 4, 4, 1, "1x2/1x5/1x5")),
                                         new monster("dragon", 100, ISMEAN, new stats(10, 5000, 10, -1, 1, "1x8/1x8/3x10")),
                                         new monster("emu", 0, ISMEAN, new stats(10, 2, 1, 7, 1, "1x2")),
                                         new monster("venus flytrap", 0, ISMEAN, new stats(10, 80, 8, 3, 1, "%%%x0")),
                                         /* NOTE: the damage is %%% so that xstr won't merge this */
                                         /* string with others, since it is written on in the program */
                                         new monster("griffin", 20, ISMEAN | ISFLY | ISREGEN, new stats(10, 2000, 13, 2, 1, "4x3/3x5")),
                                         new monster("hobgoblin", 0, ISMEAN, new stats(10, 3, 1, 5, 1, "1x8")),
                                         new monster("ice monster", 0, 0, new stats(10, 5, 1, 9, 1, "0x0")),
                                         new monster("jabberwock", 70, 0, new stats(10, 3000, 15, 6, 1, "2x12/2x4")),
                                         new monster("kestrel", 0, ISMEAN | ISFLY, new stats(10, 1, 1, 7, 1, "1x4")),
                                         new monster("leprechaun", 0, 0, new stats(10, 10, 3, 8, 1, "1x1")),
                                         new monster("medusa", 40, ISMEAN, new stats(10, 200, 8, 2, 1, "3x4/3x4/2x5")),
                                         new monster("nymph", 100, 0, new stats(10, 37, 3, 9, 1, "0x0")),
                                         new monster("orc", 15, ISGREED, new stats(10, 5, 1, 6, 1, "1x8")),
                                         new monster("phantom", 0, ISINVIS, new stats(10, 120, 8, 3, 1, "4x4")),
                                         new monster("quagga", 0, ISMEAN, new stats(10, 15, 3, 3, 1, "1x5/1x5")),
                                         new monster("rattlesnake", 0, ISMEAN, new stats(10, 9, 2, 3, 1, "1x6")),
                                         new monster("snake", 0, ISMEAN, new stats(10, 2, 1, 5, 1, "1x3")),
                                         new monster("troll", 50, ISREGEN | ISMEAN, new stats(10, 120, 6, 4, 1, "1x8/1x8/2x6")),
                                         new monster("black unicorn", 0, ISMEAN, new stats(10, 190, 7, -2, 1, "1x9/1x9/2x9")),
                                         new monster("vampire", 20, ISREGEN | ISMEAN, new stats(10, 350, 8, 1, 1, "1x10")),
                                         new monster("wraith", 0, 0, new stats(10, 55, 5, 4, 1, "1x6")),
                                         new monster("xeroc", 30, 0, new stats(10, 100, 7, 7, 1, "4x4")),
                                         new monster("yeti", 30, 0, new stats(10, 50, 4, 6, 1, "1x6/1x6")),
                                         new monster("zombie", 0, ISMEAN, new stats(10, 6, 2, 8, 1, "1x8"))
                                     };

    private obj_info[] things = new[]
                                    {
                                        new obj_info(26), /* potion */
                                        new obj_info(36), /* scroll */
                                        new obj_info(16), /* food */
                                        new obj_info(7), /* weapon */
                                        new obj_info(7), /* armor */
                                        new obj_info(4), /* ring */
                                        new obj_info(4), /* stick */
                                    };

    private obj_info[] arm_info = new[]
                                      {
                                          new obj_info("leather armor", 20, 20, null, false),
                                          new obj_info("ring mail", 15, 25, null, false),
                                          new obj_info("studded leather armor", 15, 20, null, false),
                                          new obj_info("scale mail", 13, 30, null, false),
                                          new obj_info("chain mail", 12, 75, null, false),
                                          new obj_info("splint mail", 10, 80, null, false),
                                          new obj_info("banded mail", 10, 90, null, false),
                                          new obj_info("plate mail", 5, 150, null, false),
                                      };

    private obj_info[] pot_info = new[]
                                      {
                                          new obj_info("confusion", 7, 5, null, false),
                                          new obj_info("hallucination", 8, 5, null, false),
                                          new obj_info("poison", 8, 5, null, false),
                                          new obj_info("gain strength", 13, 150, null, false),
                                          new obj_info("see invisible", 3, 100, null, false),
                                          new obj_info("healing", 13, 130, null, false),
                                          new obj_info("monster detection", 6, 130, null, false),
                                          new obj_info("magic detection", 6, 105, null, false),
                                          new obj_info("raise level", 2, 250, null, false),
                                          new obj_info("extra healing", 5, 200, null, false),
                                          new obj_info("haste self", 5, 190, null, false),
                                          new obj_info("restore strength", 13, 130, null, false),
                                          new obj_info("blindness", 5, 5, null, false),
                                          new obj_info("levitation", 6, 75, null, false),
                                      };

    private obj_info[] ring_info = new[]
                                       {
                                           new obj_info("protection", 9, 400, null, false),
                                           new obj_info("add strength", 9, 400, null, false),
                                           new obj_info("sustain strength", 5, 280, null, false),
                                           new obj_info("searching", 10, 420, null, false),
                                           new obj_info("see invisible", 10, 310, null, false),
                                           new obj_info("adornment", 1, 10, null, false),
                                           new obj_info("aggravate monster", 10, 10, null, false),
                                           new obj_info("dexterity", 8, 440, null, false),
                                           new obj_info("increase damage", 8, 400, null, false),
                                           new obj_info("regeneration", 4, 460, null, false),
                                           new obj_info("slow digestion", 9, 240, null, false),
                                           new obj_info("teleportation", 5, 30, null, false),
                                           new obj_info("stealth", 7, 470, null, false),
                                           new obj_info("maintain armor", 5, 380, null, false)
                                       };

    private obj_info[] scr_info = new[]
                                      {
                                          new obj_info("monster confusion", 7, 140, null, false),
                                          new obj_info("magic mapping", 4, 150, null, false),
                                          new obj_info("hold monster", 2, 180, null, false),
                                          new obj_info("sleep", 3, 5, null, false),
                                          new obj_info("enchant armor", 7, 160, null, false),
                                          new obj_info("identify potion", 10, 80, null, false),
                                          new obj_info("identify scroll", 10, 80, null, false),
                                          new obj_info("identify weapon", 6, 80, null, false),
                                          new obj_info("identify armor", 7, 100, null, false),
                                          new obj_info("identify ring, wand or staff", 10, 115, null, false),
                                          new obj_info("scare monster", 3, 200, null, false),
                                          new obj_info("food detection", 2, 60, null, false),
                                          new obj_info("teleportation", 5, 165, null, false),
                                          new obj_info("enchant weapon", 8, 150, null, false),
                                          new obj_info("create monster", 4, 75, null, false),
                                          new obj_info("remove curse", 7, 105, null, false),
                                          new obj_info("aggravate monsters", 3, 20, null, false),
                                          new obj_info("protect armor", 2, 250, null, false),
                                      };

    private obj_info[] weap_info = new[]
                                       {
                                           new obj_info("mace", 11, 8, null, false),
                                           new obj_info("long sword", 11, 15, null, false),
                                           new obj_info("short bow", 12, 15, null, false),
                                           new obj_info("arrow", 12, 1, null, false),
                                           new obj_info("dagger", 8, 3, null, false),
                                           new obj_info("two handed sword", 10, 75, null, false),
                                           new obj_info("dart", 12, 2, null, false),
                                           new obj_info("shuriken", 12, 5, null, false),
                                           new obj_info("spear", 12, 5, null, false),
                                           new obj_info(0), /* DO NOT REMOVE: fake entry for dragon's breath */
                                       };

    private obj_info[] ws_info = new[]
                                     {
                                         new obj_info("light", 12, 250, null, false),
                                         new obj_info("invisibility", 6, 5, null, false),
                                         new obj_info("lightning", 3, 330, null, false),
                                         new obj_info("fire", 3, 330, null, false),
                                         new obj_info("cold", 3, 330, null, false),
                                         new obj_info("polymorph", 15, 310, null, false),
                                         new obj_info("magic missile", 10, 170, null, false),
                                         new obj_info("haste monster", 10, 5, null, false),
                                         new obj_info("slow monster", 11, 350, null, false),
                                         new obj_info("drain life", 9, 300, null, false),
                                         new obj_info("nothing", 1, 5, null, false),
                                         new obj_info("teleport away", 6, 340, null, false),
                                         new obj_info("teleport to", 6, 50, null, false),
                                         new obj_info("cancellation", 5, 280, null, false),
                                     };

    private h_list[] helpstr = {
                                   new h_list('?', "	prints help", true),
                                   new h_list('/', "	identify object", true),
                                   new h_list('h', "	left", true),
                                   new h_list('j', "	down", true),
                                   new h_list('k', "	up", true),
                                   new h_list('l', "	right", true),
                                   new h_list('y', "	up & left", true),
                                   new h_list('u', "	up & right", true),
                                   new h_list('b', "	down & left", true),
                                   new h_list('n', "	down & right", true),
                                   new h_list('H', "	run left", false),
                                   new h_list('J', "	run down", false),
                                   new h_list('K', "	run up", false),
                                   new h_list('L', "	run right", false),
                                   new h_list('Y', "	run up & left", false),
                                   new h_list('U', "	run up & right", false),
                                   new h_list('B', "	run down & left", false),
                                   new h_list('N', "	run down & right", false),
                                   new h_list(CTRL('H'), "	run left until adjacent", false),
                                   new h_list(CTRL('J'), "	run down until adjacent", false),
                                   new h_list(CTRL('K'), "	run up until adjacent", false),
                                   new h_list(CTRL('L'), "	run right until adjacent", false),
                                   new h_list(CTRL('Y'), "	run up & left until adjacent", false),
                                   new h_list(CTRL('U'), "	run up & right until adjacent", false),
                                   new h_list(CTRL('B'), "	run down & left until adjacent", false),
                                   new h_list(CTRL('N'), "	run down & right until adjacent", false),
                                   new h_list('\0', "	<SHIFT><dir>: run that way", true),
                                   new h_list('\0', "	<CTRL><dir>: run till adjacent", true),
                                   new h_list('f', "<dir>	fight till death or near death", true),
                                   new h_list('t', "<dir>	throw something", true),
                                   new h_list('m', "<dir>	move onto without picking up", true),
                                   new h_list('z', "<dir>	zap a wand in a direction", true),
                                   new h_list('^', "<dir>	identify trap type", true),
                                   new h_list('s', "	search for trap/secret door", true),
                                   new h_list('>', "	go down a staircase", true),
                                   new h_list('<', "	go up a staircase", true),
                                   new h_list('.', "	rest for a turn", true),
                                   new h_list(',', "	pick something up", true),
                                   new h_list('i', "	inventory", true),
                                   new h_list('I', "	inventory single item", true),
                                   new h_list('q', "	quaff potion", true),
                                   new h_list('r', "	read scroll", true),
                                   new h_list('e', "	eat food", true),
                                   new h_list('w', "	wield a weapon", true),
                                   new h_list('W', "	wear armor", true),
                                   new h_list('T', "	take armor off", true),
                                   new h_list('P', "	put on ring", true),
                                   new h_list('R', "	remove ring", true),
                                   new h_list('d', "	drop object", true),
                                   new h_list('c', "	call object", true),
                                   new h_list('a', "	repeat last command", true),
                                   new h_list(')', "	print current weapon", true),
                                   new h_list(']', "	print current armor", true),
                                   new h_list('=', "	print current rings", true),
                                   new h_list('@', "	print current stats", true),
                                   new h_list('D', "	recall what's been discovered", true),
                                   new h_list('o', "	examine/set options", true),
                                   new h_list(CTRL('R'), "	redraw screen", true),
                                   new h_list(CTRL('P'), "	repeat last message", true),
                                   new h_list(ESCAPE, "	cancel command", true),
                                   new h_list('S', "	save game", true),
                                   new h_list('Q', "	quit", true),
                                   new h_list('!', "	shell escape", true),
                                   new h_list('F', "<dir>	fight till either of you dies", true),
                                   new h_list('v', "	print version number", true),
                               };
}
