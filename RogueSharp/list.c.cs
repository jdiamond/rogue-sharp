/*
 * Functions for dealing with linked lists of goodies
 *
 * @(#)list.c	4.12 (Berkeley) 02/05/99
 *
 * Rogue: Exploring the Dungeons of Doom
 * Copyright (C) 1980-1983, 1985, 1999 Michael Toy, Ken Arnold and Glenn Wichman
 * All rights reserved.
 *
 * See the file LICENSE.TXT for full copyright and licensing information.
 */

partial class Rogue
{
    // takes an item out of whatever linked list it might be in
    void detach(ref THING list, THING item)
    {
        if (list == item)
            list = next(item);
        if (prev(item) != null)
            item.l_prev.l_next = next(item);
        if (next(item) != null)
            item.l_next.l_prev = prev(item);
        item.l_next = null;
        item.l_prev = null;
    }

    // add an item to the head of a list
    void attach(ref THING list, THING item)
    {
        if (list != null)
        {
            item.l_next = list;
            list.l_prev = item;
            item.l_prev = null;
        }
        else
        {
            item.l_next = null;
            item.l_prev = null;
        }
        list = item;
    }

    // Throw the whole blamed thing away
    void free_list(ref THING ptr)
    {
        THING item;

        while (ptr != null)
        {
            item = ptr;
            ptr = next(item);
            discard(item);
        }
    }

    // Free up an item
    void discard(THING item)
    {
    }

    // Get a new item with a specified size
    THING new_item()
    {
        return new THING();
    }
}
