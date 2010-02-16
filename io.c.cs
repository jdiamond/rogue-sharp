/*
 * Various input/output functions
 *
 * @(#)io.c	4.32 (Berkeley) 02/05/99
 */

using System;

partial class Rogue
{
    private int MAXMSG = NUMCOLS - "--More--".Length;
    static string msgbuf = "";
    static int newpos = 0;

    // Display a message at the top of the screen.
    int msg(string fmt, params object[] args)
    {
        /*
         * if the string is "", just clear the line
         */
        if (string.IsNullOrEmpty(fmt))
        {
            move(0, 0);
            clrtoeol();
            mpos = 0;
            return ~ESCAPE;
        }
        /*
         * otherwise add to the message and flush it out
         */
        doadd(fmt, args);
        return endmsg();
    }

    // Add things to the current message
    void addmsg(string fmt, params object[] args)
    {
        doadd(fmt, args);
    }

    // Display a new msg (giving him a chance to see the previous one
    // if it is up there with the --More--)
    int endmsg()
    {
        char ch;

        if (save_msg)
            huh = msgbuf;

        if (mpos != 0)
        {
            look(false);
            mvaddstr(0, mpos, "--More--");
            refresh();
            if (!msg_esc)
                wait_for(' ');
            else
            {
                while ((ch = readchar()) != ' ')
                    if (ch == ESCAPE)
                    {
                        msgbuf = "";
                        mpos = 0;
                        newpos = 0;
                        return ESCAPE;
                    }
            }
        }
        /*
         * All messages should start with uppercase, except ones that
         * start with a pack addressing character
         */
        if (!string.IsNullOrEmpty(msgbuf) && char.IsLower(msgbuf[0]) && !lower_msg && msgbuf[1] != ')')
            msgbuf = msgbuf.Substring(0, 1).ToUpper() + msgbuf.Substring(1);
        mvaddstr(0, 0, msgbuf);
        for (int i = msgbuf.Length; i < MAXCOLS; i++)
            mvaddch(0, i, ' ');
        mpos = newpos;
        newpos = 0;
        msgbuf = "";
        refresh();
        return ~ESCAPE;
    }

    // Perform an add onto the message buffer
    void doadd(string fmt, object[] args)
    {
        /*
         * Do the printf into buf
         */
        string buf = string.Format(fmt, args);
        if (buf.Length + newpos >= MAXMSG)
            endmsg();
        msgbuf += buf;
        newpos = msgbuf.Length;
    }

    // Returns true if it is ok to step on ch
    bool step_ok(char ch)
    {
        switch (ch)
        {
            case ' ':
            case '|':
            case '-':
                return false;
            default:
                return (!char.IsLetter(ch));
        }
    }

    // Reads and returns a character, checking for gross input errors
    char readchar()
    {
        var keyInfo = Console.ReadKey(true);
        if ((keyInfo.Modifiers & ConsoleModifiers.Control) != 0)
        {
            switch (keyInfo.Key)
            {
                case ConsoleKey.LeftArrow:
                    return CTRL('H');
                case ConsoleKey.RightArrow:
                    return CTRL('L');
                case ConsoleKey.UpArrow:
                    return CTRL('K');
                case ConsoleKey.DownArrow:
                    return CTRL('J');
            }
        }
        if ((keyInfo.Modifiers & ConsoleModifiers.Shift) != 0)
        {
            switch (keyInfo.Key)
            {
                case ConsoleKey.LeftArrow:
                    return 'H';
                case ConsoleKey.RightArrow:
                    return 'L';
                case ConsoleKey.UpArrow:
                    return 'K';
                case ConsoleKey.DownArrow:
                    return 'J';
            }
        }
        switch (keyInfo.Key)
        {
            case ConsoleKey.LeftArrow:
                return 'h';
            case ConsoleKey.RightArrow:
                return 'l';
            case ConsoleKey.UpArrow:
                return 'k';
            case ConsoleKey.DownArrow:
                return 'j';
        }
        return keyInfo.KeyChar;
    }

    static int hpwidth = 0;
    static int s_hungry = 0;
    static int s_lvl = 0;
    static int s_pur = -1;
    static int s_hp = 0;
    static int s_arm = 0;
    static int s_str = 0;
    static int s_exp = 0;
    static string[] state_name =
    {
        "", "Hungry", "Weak", "Faint"
    };

    // Display the important stats line.  Keep the cursor where it was.
    void status()
    {
        int oy, ox, temp;

        /*
         * If nothing has changed since the last status, don't
         * bother.
         */
        temp = (cur_armor != null ? cur_armor.o_arm : pstats.s_arm);
        if (s_hp == pstats.s_hpt && s_exp == pstats.s_exp && s_pur == purse
            && s_arm == temp && s_str == pstats.s_str && s_lvl == level
            && s_hungry == hungry_state
            && !stat_msg)
            return;

        s_arm = temp;

        getyx(stdscr, out oy, out ox);
        if (s_hp != max_hp)
        {
            temp = max_hp;
            s_hp = max_hp;
            for (hpwidth = 0; temp != 0; hpwidth++)
                temp /= 10;
        }

        /*
         * Save current status
         */
        s_lvl = level;
        s_pur = purse;
        s_hp = pstats.s_hpt;
        s_str = pstats.s_str;
        s_exp = pstats.s_exp; 
        s_hungry = hungry_state;

        if (stat_msg)
        {
            move(0, 0);
            msg("Level: {0}  Gold: {1,-5}  Hp: {2}({3})  Str: {4,2}({5})  Arm: {6,-2}  Exp: {7}/{8}  {9}",
            level, purse, pstats.s_hpt.ToString().PadLeft(hpwidth), max_hp.ToString().PadLeft(hpwidth), pstats.s_str,
            max_stats.s_str, 10 - s_arm, pstats.s_lvl, pstats.s_exp,
            state_name[hungry_state]);
        }
        else
        {
            move(STATLINE, 0);

            printw("Level: {0}  Gold: {1,-5}  Hp: {2}({3})  Str: {4,2}({5})  Arm: {6,-2}  Exp: {7}/{8}  {9}",
            level, purse, pstats.s_hpt.ToString().PadLeft(hpwidth), max_hp.ToString().PadLeft(hpwidth), pstats.s_str,
            max_stats.s_str, 10 - s_arm, pstats.s_lvl, pstats.s_exp,
            state_name[hungry_state]);
        }

        clrtoeol();
        move(oy, ox);
    }

    // Sit around until the guy types the right key
    void wait_for(int ch)
    {
        char c;

        if (ch == '\n')
            while ((c = readchar()) != '\n' && c != '\r')
                continue;
        else
            while (readchar() != ch)
                continue;
    }

    // Function used to display a window and wait before returning
    void show_win(string message)
    {
        IntPtr win;

        win = hw;
        wmove(win, 0, 0);
        waddstr(win, message);
        touchwin(win);
        wmove(win, hero.y, hero.x);
        wrefresh(win);
        wait_for(' ');
        clearok(curscr, true);
        touchwin(stdscr);
    }
}