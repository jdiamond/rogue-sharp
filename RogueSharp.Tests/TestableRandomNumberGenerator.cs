using System.Collections.Generic;

namespace RogueSharp.Tests
{
    public class TestableRandomNumberGenerator : RandomNumberGenerator
    {
        private Dictionary<string, RandomNumberList> randomResults = new Dictionary<string, RandomNumberList>();
        
        public override int Next(string key, int range)
        {
            if (key != null)
            {
                RandomNumberList numbers;

                if (this.randomResults.TryGetValue(key, out numbers))
                    return numbers.Next();
            }

            return base.Next(key, range);
        }

        public void Set(string key, int[] numbers)
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

            public int Next()
            {
                int n = this.numbers[this.nextIndex];
                this.nextIndex++;
                if (this.nextIndex >= this.numbers.Length) this.nextIndex = 0;
                return n;
            }
        }
    }
}