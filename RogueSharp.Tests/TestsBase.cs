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
        private Dictionary<string, int> randomResults = new Dictionary<string, int>();

        private int RandomHook(int range, string key)
        {
            int n;

            if (this.randomResults.TryGetValue(key, out n))
                return n;

            return -1;
        }

        protected void RegisterRandomNumber(string key, int result)
        {
            this.randomResults[key] = result;
        }
    }
}