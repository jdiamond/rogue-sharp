using System;
using System.Runtime.InteropServices;

partial class Rogue
{
    public static int A_ATTRIBUTES = 0xff00;
    public static int A_CHARTEXT   = 0x00ff;
    public static int A_COLOR      = 0xf800;

    [DllImport("pdcurses.dll")]
    public static extern IntPtr initscr();

    [DllImport("pdcurses.dll")]
    public static extern int move(int y, int x);

    [DllImport("pdcurses.dll")]
    public static extern int addch(int ch);

    [DllImport("pdcurses.dll")]
    public static extern int waddch(IntPtr win, int ch);

    [DllImport("pdcurses.dll")]
    public static extern int mvaddch(int y, int x, int ch);

    [DllImport("pdcurses.dll")]
    public static extern int mvwinch(IntPtr win, int y, int x);

    [DllImport("pdcurses.dll")]
    public static extern int touchwin(IntPtr win);

    [DllImport("pdcurses.dll")]
    public static extern int wstandout(IntPtr win);

    [DllImport("pdcurses.dll")]
    public static extern int wstandend(IntPtr win);

    [DllImport("pdcurses.dll")]
    public static extern int wclrtoeol(IntPtr win);

    [DllImport("pdcurses.dll")]
    public static extern char erasechar();

    [DllImport("pdcurses.dll")]
    public static extern char killchar();

    [DllImport("pdcurses.dll")]
    public static extern int mvwin(IntPtr win, int y, int x);

    [DllImport("pdcurses.dll")]
    public static extern int mvinch(int y, int x);

    [DllImport("pdcurses.dll")]
    public static extern int mvaddstr(int y, int x, string str);

    [DllImport("pdcurses.dll")]
    public static extern int mvwaddstr(IntPtr win, int y, int x, string str);

    [DllImport("pdcurses.dll")]
    public static extern int waddstr(IntPtr win, string str);

    [DllImport("pdcurses.dll")]
    public static extern int addstr(string str);

    [DllImport("pdcurses.dll")]
    public static extern int clearok(IntPtr win, bool bf);

    [DllImport("pdcurses.dll")]
    public static extern int leaveok(IntPtr win, bool bf);

    public static int mvwprintw(IntPtr win, int y, int x, string fmt, params object[] args)
    {
        return mvwaddstr(win, y, x, string.Format(fmt, args));
    }

    public static int mvprintw(int y, int x, string fmt, params object[] args)
    {
        return mvaddstr(y, x, string.Format(fmt, args));
    }

    public static int printw(string fmt, params object[] args)
    {
        return addstr(string.Format(fmt, args));
    }

    [DllImport("pdcurses.dll")]
    public static extern int clear();

    [DllImport("pdcurses.dll")]
    public static extern int refresh();

    [DllImport("pdcurses.dll")]
    public static extern int inch();

    [DllImport("pdcurses.dll")]
    public static extern int standend();

    [DllImport("pdcurses.dll")]
    public static extern int standout();

    [DllImport("pdcurses.dll")]
    public static extern string unctrl(char c);

    [DllImport("pdcurses.dll")]
    public static extern int clrtoeol();

    [DllImport("pdcurses.dll")]
    public static extern int getcury(IntPtr win);

    [DllImport("pdcurses.dll")]
    public static extern int getcurx(IntPtr win);

    [DllImport("pdcurses.dll")]
    public static extern int wclear(IntPtr win);

    [DllImport("pdcurses.dll")]
    public static extern int wrefresh(IntPtr win);

    [DllImport("pdcurses.dll")]
    public static extern int delwin(IntPtr win);

    [DllImport("pdcurses.dll")]
    public static extern int werase(IntPtr win);

    [DllImport("pdcurses.dll")]
    public static extern IntPtr newwin(int nlines, int ncols, int begin_y, int begin_x);

    [DllImport("pdcurses.dll")]
    public static extern IntPtr subwin(IntPtr orig, int nlines, int ncols, int begin_y, int begin_x);

    [DllImport("pdcurses.dll")]
    public static extern int wmove(IntPtr win, int y, int x);

    public static void getyx(IntPtr w, out int y, out int x)
    {
        y = getcury(w);
        x = getcurx(w);
    }

    public static IntPtr stdscr
    {
        get { return GetIntPtr("stdscr"); }
    }

    public static IntPtr curscr
    {
        get { return GetIntPtr("curscr"); }
    }

    public static int LINES
    {
        get { return GetInt32("LINES"); }
    }

    public static int COLS
    {
        get { return GetInt32("COLS"); }
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr LoadLibrary(string lpszLib);

    private static IntPtr GetAddress(string name)
    {
        IntPtr handle = LoadLibrary("pdcurses.dll");

        if (handle == IntPtr.Zero)
            throw new InvalidOperationException();

        IntPtr address = GetProcAddress(handle, name);

        if (address == IntPtr.Zero)
            throw new InvalidOperationException();

        return address;
    }

    private static IntPtr GetIntPtr(string name)
    {
        return Marshal.ReadIntPtr(GetAddress(name));
    }

    private static int GetInt32(string name)
    {
        return Marshal.ReadInt16(GetAddress(name));
    }
}
