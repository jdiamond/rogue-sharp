namespace RogueSharp.Tests
{
    public class TestsBase
    {
        public TestsBase()
        {
            this.rng = new TestableRandomNumberGenerator();
            this.rogue = new Rogue(this.rng);
        }

        protected TestableRandomNumberGenerator rng;
        protected Rogue rogue;

        protected void RegisterRandomNumber(string key, int number)
        {
            this.RegisterRandomNumbers(key, number);
        }

        protected void RegisterRandomNumbers(string key, params int[] numbers)
        {
            this.rng.Set(key, numbers);
        }
    }
}