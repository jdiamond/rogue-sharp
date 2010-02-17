using System.Collections.Generic;

namespace RogueSharp.Tests
{
    public class TestsBase
    {
        public TestsBase()
        {
            this.rogue = new Rogue();
            this.rogue.rndhook = this.RandomHook;
        }

        protected Rogue rogue;
        private Dictionary<string, RandomNumberList> randomResults = new Dictionary<string, RandomNumberList>();

        private int RandomHook(int range, string key)
        {
            RandomNumberList numbers;

            if (this.randomResults.TryGetValue(key, out numbers))
                return numbers.GetNextNumber();

            return -1;
        }

        protected void RegisterRandomNumber(string key, int number)
        {
            this.RegisterRandomNumbers(key, number);
        }

        protected void RegisterRandomNumbers(string key, params int[] numbers)
        {
            this.randomResults[key] = new RandomNumberList(numbers);
        }

        private class RandomNumberList
        {
            public RandomNumberList(int[] numbers)
            {
                this.numbers = numbers;
            }

            private int[] numbers;
            private int nextIndex;

            public int GetNextNumber()
            {
                int n = this.numbers[this.nextIndex];
                this.nextIndex++;
                if (this.nextIndex >= this.numbers.Length) this.nextIndex = 0;
                return n;
            }
        }
    }
}