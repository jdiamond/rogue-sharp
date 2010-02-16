/*
 * Contains functions for dealing with things that happen in the
 * future.
 *
 * @(#)daemon.c	4.7 (Berkeley) 02/05/99
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
    private int EMPTY = 0;
    private int DAEMON = -1;

    private delayed_action[] d_list = new delayed_action[MAXDAEMONS]
                                          {
                                              new delayed_action(),
                                              new delayed_action(),
                                              new delayed_action(),
                                              new delayed_action(),
                                              new delayed_action(),
                                              new delayed_action(),
                                              new delayed_action(),
                                              new delayed_action(),
                                              new delayed_action(),
                                              new delayed_action(),
                                              new delayed_action(),
                                              new delayed_action(),
                                              new delayed_action(),
                                              new delayed_action(),
                                              new delayed_action(),
                                              new delayed_action(),
                                              new delayed_action(),
                                              new delayed_action(),
                                              new delayed_action(),
                                              new delayed_action(),
                                          };

    // Find an empty slot in the daemon/fuse list
    delayed_action d_slot()
    {
        foreach (var dev in d_list)
            if (dev.d_type == EMPTY)
                return dev;
        return null;
    }

    // Find a particular slot in the table
    delayed_action find_slot(Action<int> func)
    {
        foreach (var dev in d_list)
            if (dev.d_type != EMPTY && func == dev.d_func)
                return dev;

        return null;
    }

    // Start a daemon, takes a function.
    void start_daemon(Action<int> func, int arg, int type)
    {
        delayed_action dev;

        dev = d_slot();
        dev.d_type = type;
        dev.d_func = func;
        dev.d_arg = arg;
        dev.d_time = DAEMON;
    }

    // Remove a daemon from the list
    void kill_daemon(Action<int> func)
    {
        delayed_action dev;

        if ((dev = find_slot(func)) == null)
            return;
        /*
         * Take it out of the list
         */
        dev.d_type = EMPTY;
    }

    // Run all the daemons that are active with the current flag,
    // passing the argument to the function.
    void do_daemons(int flag)
    {
        /*
         * Loop through the devil list
         */
        foreach (var dev in d_list)
            /*
             * Executing each one, giving it the proper arguments
             */
            if (dev.d_type == flag && dev.d_time == DAEMON)
                dev.d_func(dev.d_arg);
    }

    // Start a fuse to go off in a certain number of turns
    void fuse(Action<int> func, int arg, int time, int type)
    {
        delayed_action wire;

        wire = d_slot();
        wire.d_type = type;
        wire.d_func = func;
        wire.d_arg = arg;
        wire.d_time = time;
    }

    // Increase the time until a fuse goes off
    void lengthen(Action<int> func, int xtime)
    {
        delayed_action wire;

        if ((wire = find_slot(func)) == null)
            return;
        wire.d_time += xtime;
    }

    // Put out a fuse
    void extinguish(Action<int> func)
    {
        delayed_action wire;

        if ((wire = find_slot(func)) == null)
            return;
        wire.d_type = EMPTY;
    }

    // Decrement counters and start needed fuses
    void do_fuses(int flag)
    {
        /*
         * Step though the list
         */
        foreach (var wire in d_list)
        {
            /*
             * Decrementing counters and starting things we want.  We also need
             * to remove the fuse from the list once it has gone off.
             */
            if (flag == wire.d_type && wire.d_time > 0 && --wire.d_time == 0)
            {
                wire.d_type = EMPTY;
                wire.d_func(wire.d_arg);
            }
        }
    }
}