using System.Linq;
using SharpTestsEx;
using Xunit;

namespace RogueSharp.Tests
{
    public class NewLevel_Room_Tests : TestsBase
    {
        [Fact]
        public void Creating_a_new_level_marks_some_rooms_as_gone()
        {
            RegisterRandomNumber("gone rooms", 3);

            rogue.new_level();

            rogue.rooms
                .Count(r => (r.r_flags & Rogue.ISGONE) != 0)
                .Should().Be(3);
        }
    }
}
