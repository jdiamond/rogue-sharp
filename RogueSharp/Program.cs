using RogueSharp;

public partial class Rogue
{
    private static void Main(string[] args)
    {
        var rogue = new Rogue();
        rogue.main();
    }

    public Rogue()
        : this(new RandomNumberGenerator())
    {
    }

    public Rogue(IRandomNumberGenerator rng)
    {
        InitActions();
        this.rng = rng;
    }

    private IRandomNumberGenerator rng;
}
