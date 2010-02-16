/*
 * Special wizard commands (some of which are also non-wizard commands
 * under strange circumstances)
 *
 * @(#)wizard.c	4.30 (Berkeley) 02/05/99
 *
 * Rogue: Exploring the Dungeons of Doom
 * Copyright (C) 1980-1983, 1985, 1999 Michael Toy, Ken Arnold and Glenn Wichman
 * All rights reserved.
 *
 * See the file LICENSE.TXT for full copyright and licensing information.
 */

partial class Rogue
{
    // What a certin object is
    void whatis(bool insist, int type)
    {
        THING obj;

        if (pack == null)
        {
            msg("you don't have anything in your pack to identify");
            return;
        }

        for (; ; )
        {
            obj = get_item("identify", type);
            if (insist)
            {
                if (n_objs == 0)
                    return;
                else if (obj == null)
                    msg("you must identify something");
                else if (type != 0 && obj.o_type != type &&
                   !(type == R_OR_S && (obj.o_type == RING || obj.o_type == STICK)))
                    msg("you must identify a {0}", type_name(type));
                else
                    break;
            }
            else
                break;
        }

        if (obj == null)
            return;

        switch (obj.o_type)
        {
            case SCROLL:
                set_know(obj, scr_info);
                break;
            case POTION:
                set_know(obj, pot_info);
                break;
            case STICK:
                set_know(obj, ws_info);
                break;
            case WEAPON:
            case ARMOR:
                obj.o_flags |= ISKNOW;
                break;
            case RING:
                set_know(obj, ring_info);
                break;
        }
        msg(inv_name(obj, false));
    }

    // Set things up when we really know what a thing is
    void set_know(THING obj, obj_info[] info)
    {
        string guess;

        info[obj.o_which].oi_know = true;
        obj.o_flags |= ISKNOW;
        guess = info[obj.o_which].oi_guess;
        if (!string.IsNullOrEmpty(guess))
        {
            info[obj.o_which].oi_guess = null;
        }
    }

    // Return a pointer to the name of the type
    string type_name(int type)
    {
        h_list[] tlist = new[] {
        new h_list(POTION, "potion",		false),
        new h_list(SCROLL, "scroll",		false),
        new h_list(FOOD,	 "food",		false),
        new h_list(R_OR_S, "ring, wand or staff",	false),
        new h_list(RING,	 "ring",		false),
        new h_list(STICK,	 "wand or staff",	false),
        new h_list(WEAPON, "weapon",		false),
        new h_list(ARMOR,	 "suit of armor",	false),
        };

        foreach (var hp in tlist)
            if (type == hp.h_ch)
                return hp.h_desc;
        /* NOTREACHED */
        return null;
    }

    // Bamf the hero someplace else
    void teleport()
    {
        coord c = new coord();

        mvaddch(hero.y, hero.x, floor_at());
        find_floor(null, out c, 0, true);
        if (roomin(c) != proom)
        {
            leave_room(hero);
            hero.CopyFrom(c);
            enter_room(hero);
        }
        else
        {
            hero.CopyFrom(c);
            look(true);
        }
        mvaddch(hero.y, hero.x, PLAYER);
        /*
         * turn off ISHELD in case teleportation was done while fighting
         * a Flytrap
         */
        if (on(player, ISHELD))
        {
            player.t_flags &= ~ISHELD;
            vf_hit = 0;
            monsters['F' - 'A'].m_stats.s_dmg = "000x0";
        }
        no_move = 0;
        count = 0;
        running = false;
        //flush_type();
    }

    // Print out the map for the wizard
    void show_map()
    {
        int y, x, real;

        wclear(hw);
        for (y = 1; y < NUMLINES - 1; y++)
            for (x = 0; x < NUMCOLS; x++)
            {
                real = flat(y, x);
                if ((real & F_REAL) == 0)
                    wstandout(hw);
                wmove(hw, y, x);
                waddch(hw, chat(y, x));
                if (real != 0)
                    wstandend(hw);
            }
        show_win("---More (level map)---");
    }
}