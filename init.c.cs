/*
 * global variable initializaton
 *
 * @(#)init.c	4.31 (Berkeley) 02/05/99
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
    // Roll her up
    void init_player()
    {
        THING obj;

        pstats = max_stats.Copy();
        food_left = HUNGERTIME;
        /*
         * Give him some food
         */
        obj = new_item();
        obj.o_type = FOOD;
        obj.o_count = 1;
        add_pack(obj, true);
        /*
         * And his suit of armor
         */
        obj = new_item();
        obj.o_type = ARMOR;
        obj.o_which = RING_MAIL;
        obj.o_arm = a_class[RING_MAIL] - 1;
        obj.o_flags |= ISKNOW;
        obj.o_count = 1;
        cur_armor = obj;
        add_pack(obj, true);
        /*
         * Give him his weaponry.  First a mace.
         */
        obj = new_item();
        init_weapon(obj, MACE);
        obj.o_hplus = 1;
        obj.o_dplus = 1;
        obj.o_flags |= ISKNOW;
        add_pack(obj, true);
        cur_weapon = obj;
        /*
         * Now a +1 bow
         */
        obj = new_item();
        init_weapon(obj, BOW);
        obj.o_hplus = 1;
        obj.o_flags |= ISKNOW;
        add_pack(obj, true);
        /*
         * Now some arrows
         */
        obj = new_item();
        init_weapon(obj, ARROW);
        obj.o_count = rnd(15) + 25;
        obj.o_flags |= ISKNOW;
        add_pack(obj, true);
    }

    // Contains defintions and functions for dealing with things like
    // potions and scrolls
    static string[] rainbow = new[] {
        "amber",
        "aquamarine",
        "black",
        "blue",
        "brown",
        "clear",
        "crimson",
        "cyan",
        "ecru",
        "gold",
        "green",
        "grey",
        "magenta",
        "orange",
        "pink",
        "plaid",
        "purple",
        "red",
        "silver",
        "tan",
        "tangerine",
        "topaz",
        "turquoise",
        "vermilion",
        "violet",
        "white",
        "yellow",
    };

    private static int NCOLORS = rainbow.Length;
    int cNCOLORS = NCOLORS;

    static string[] sylls = new[] {
        "a", "ab", "ag", "aks", "ala", "an", "app", "arg", "arze", "ash",
        "bek", "bie", "bit", "bjor", "blu", "bot", "bu", "byt", "comp",
        "con", "cos", "cre", "dalf", "dan", "den", "do", "e", "eep", "el",
        "eng", "er", "ere", "erk", "esh", "evs", "fa", "fid", "fri", "fu",
        "gan", "gar", "glen", "gop", "gre", "ha", "hyd", "i", "ing", "ip",
        "ish", "it", "ite", "iv", "jo", "kho", "kli", "klis", "la", "lech",
        "mar", "me", "mi", "mic", "mik", "mon", "mung", "mur", "nej",
        "nelg", "nep", "ner", "nes", "nes", "nih", "nin", "o", "od", "ood",
        "org", "orn", "ox", "oxy", "pay", "ple", "plu", "po", "pot",
        "prok", "re", "rea", "rhov", "ri", "ro", "rog", "rok", "rol", "sa",
        "san", "sat", "sef", "seh", "shu", "ski", "sna", "sne", "snik",
        "sno", "so", "sol", "sri", "sta", "sun", "ta", "tab", "tem",
        "ther", "ti", "tox", "trol", "tue", "turs", "u", "ulk", "um", "un",
        "uni", "ur", "val", "viv", "vly", "vom", "wah", "wed", "werg",
        "wex", "whon", "wun", "xo", "y", "yot", "yu", "zant", "zeb", "zim",
        "zok", "zon", "zum",
    };

    static STONE[] stones = new [] {
        new STONE( "agate",		 25),
        new STONE( "alexandrite",	 40),
        new STONE( "amethyst",	 50),
        new STONE( "carnelian",	 40),
        new STONE( "diamond",	300),
        new STONE( "emerald",	300),
        new STONE( "germanium",	225),
        new STONE( "granite",	  5),
        new STONE( "garnet",		 50),
        new STONE( "jade",		150),
        new STONE( "kryptonite",	300),
        new STONE( "lapis lazuli",	 50),
        new STONE( "moonstone",	 50),
        new STONE( "obsidian",	 15),
        new STONE( "onyx",		 60),
        new STONE( "opal",		200),
        new STONE( "pearl",		220),
        new STONE( "peridot",	 63),
        new STONE( "ruby",		350),
        new STONE( "sapphire",	285),
        new STONE( "stibotantalite",	200),
        new STONE( "tiger eye",	 50),
        new STONE( "topaz",		 60),
        new STONE( "turquoise",	 70),
        new STONE( "taaffeite",	300),
        new STONE( "zircon",	 	 80),
    };

    private static int NSTONES = stones.Length;
    int cNSTONES = NSTONES;

    static string[] wood = new[] {
        "avocado wood",
        "balsa",
        "bamboo",
        "banyan",
        "birch",
        "cedar",
        "cherry",
        "cinnibar",
        "cypress",
        "dogwood",
        "driftwood",
        "ebony",
        "elm",
        "eucalyptus",
        "fall",
        "hemlock",
        "holly",
        "ironwood",
        "kukui wood",
        "mahogany",
        "manzanita",
        "maple",
        "oaken",
        "persimmon wood",
        "pecan",
        "pine",
        "poplar",
        "redwood",
        "rosewood",
        "spruce",
        "teak",
        "walnut",
        "zebrawood",
    };

    private static int NWOOD = wood.Length;
    int cNWOOD = NWOOD;

    static string[] metal = new[] {
        "aluminum",
        "beryllium",
        "bone",
        "brass",
        "bronze",
        "copper",
        "electrum",
        "gold",
        "iron",
        "lead",
        "magnesium",
        "mercury",
        "nickel",
        "pewter",
        "platinum",
        "steel",
        "silver",
        "silicon",
        "tin",
        "titanium",
        "tungsten",
        "zinc",
    };

    private static int NMETAL = metal.Length;
    int cNMETAL = NMETAL;

    static bool[] used = new bool[Math.Max(Math.Max(NCOLORS, NSTONES), NWOOD)];

    // Initialize the potion color scheme for this time
    public void init_colors()
    {
        int i, j;

        for (i = 0; i < NCOLORS; i++)
            used[i] = false;
        for (i = 0; i < MAXPOTIONS; i++)
        {
            do
                j = rnd(NCOLORS);
            while (used[j]);
            used[j] = true;
            p_colors[i] = rainbow[j];
        }
    }

    int MAXNAME = 40;	/* Max number of characters in a name */

    // Generate the names of the various scrolls
    public void init_names()
    {
        int nsyl;
        string cp, sp;
        int i, nwords;

        for (i = 0; i < MAXSCROLLS; i++)
        {
            cp = "";
            nwords = rnd(3) + 2;
            while (nwords-- > 0)
            {
                nsyl = rnd(3) + 1;
                while (nsyl-- > 0)
                {
                    sp = sylls[rnd(sylls.Length)];
                    if (cp.Length > MAXNAME)
                        break;
                    cp += sp + " ";
                }
            }
            s_names[i] = cp.Substring(0, cp.Length - 1);
        }
    }

    // Initialize the ring stone setting scheme for this time
    public void init_stones()
    {
        int i, j;

        for (i = 0; i < NSTONES; i++)
            used[i] = false;
        for (i = 0; i < MAXRINGS; i++)
        {
            do
                j = rnd(NSTONES);
            while (used[j]);
            used[j] = true;
            r_stones[i] = stones[j].st_name;
            ring_info[i].oi_worth += stones[j].st_value;
        }
    }

    // Initialize the construction materials for wands and staffs
    public void init_materials()
    {
        int i, j;
        string str;
        bool[] metused = new bool[NMETAL];

        for (i = 0; i < NWOOD; i++)
            used[i] = false;
        for (i = 0; i < NMETAL; i++)
            metused[i] = false;
        for (i = 0; i < MAXSTICKS; i++)
        {
            for (;;)
                if (rnd(2) == 0)
                {
                    j = rnd(NMETAL);
                    if (!metused[j])
                    {
                        ws_type[i] = "wand";
                        str = metal[j];
                        metused[j] = true;
                        break;
                    }
                }
                else
                {
                    j = rnd(NWOOD);
                    if (!used[j])
                    {
                        ws_type[i] = "staff";
                        str = wood[j];
                        used[j] = true;
                        break;
                    }
                }
            ws_made[i] = str;
        }
    }

    // Sum up the probabilities for items appearing
    void sumprobs(obj_info[] info, int bound)
    {
        for (int i = 1; i < info.Length; ++i )
            info[i].oi_prob += info[i - 1].oi_prob;
    }

    // Initialize the probabilities for the various items
    void init_probs()
    {
        sumprobs(things, NUMTHINGS);
        sumprobs(pot_info, MAXPOTIONS);
        sumprobs(scr_info, MAXSCROLLS);
        sumprobs(ring_info, MAXRINGS);
        sumprobs(ws_info, MAXSTICKS);
        sumprobs(weap_info, MAXWEAPONS);
        sumprobs(arm_info, MAXARMORS);
    }

    // If he is halucinating, pick a random color name and return it,
    // otherwise return the given color.
    string pick_color(string col)
    {
        return (on(player, ISHALU) ? rainbow[rnd(NCOLORS)] : col);
    }
}