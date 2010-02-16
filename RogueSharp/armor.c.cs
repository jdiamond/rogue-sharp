/*
 * This file contains misc functions for dealing with armor
 * @(#)armor.c	4.14 (Berkeley) 02/05/99
 *
 * Rogue: Exploring the Dungeons of Doom
 * Copyright (C) 1980-1983, 1985, 1999 Michael Toy, Ken Arnold and Glenn Wichman
 * All rights reserved.
 *
 * See the file LICENSE.TXT for full copyright and licensing information.
 */

partial class Rogue
{
    // The player wants to wear something, so let him/her put it on.
    void wear()
    {
        THING obj;
        string sp;

        if ((obj = get_item("wear", ARMOR)) == null)
            return;
        if (cur_armor != null)
        {
            addmsg("you are already wearing some");
            if (!terse)
                addmsg(".  You'll have to take it off first");
            endmsg();
            after = false;
            return;
        }
        if (obj.o_type != ARMOR)
        {
            msg("you can't wear that");
            return;
        }
        waste_time();
        obj.o_flags |= ISKNOW;
        sp = inv_name(obj, true);
        cur_armor = obj;
        if (!terse)
            addmsg("you are now ");
        msg("wearing {0}", sp);
    }

    // Get the armor off of the players back
    void take_off()
    {
        THING obj;

        if ((obj = cur_armor) == null)
        {
            after = false;
            if (terse)
                msg("not wearing armor");
            else
                msg("you aren't wearing any armor");
            return;
        }
        if (!dropcheck(cur_armor))
            return;
        cur_armor = null;
        if (terse)
            addmsg("was");
        else
            addmsg("you used to be");
        msg(" wearing {0}) {1}", obj.o_packch, inv_name(obj, true));
    }

    // Do nothing but let other things happen
    void waste_time()
    {
        do_daemons(BEFORE);
        do_fuses(BEFORE);
        do_daemons(AFTER);
        do_fuses(AFTER);
    }
}
