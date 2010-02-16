using System.Linq;
using SharpTestsEx;
using Xunit;

namespace RogueSharp.Tests
{
    public class NewLevel_Tests : TestsBase
    {
        [Fact]
        public void Creating_a_new_level_always_creates_9_rooms()
        {
            rogue.new_level();

            rogue.rooms.Length.Should().Be(9);
        }

        [Fact]
        public void Creating_a_new_level_marks_some_rooms_as_gone()
        {
            RegisterRandomNumber("gone rooms", 3);

            rogue.new_level();

            rogue.rooms.Count(r => r.IsGone).Should().Be(3);
            rogue.rooms.Count(r => !r.IsGone).Should().Be(6);
        }

        [Fact]
        public void Level_1_never_has_any_dark_rooms()
        {
            RegisterRandomNumber("is dark room", 0);

            rogue.new_level();

            rogue.rooms.Count(r => r.IsDark).Should().Be(0);
        }

        [Fact]
        public void Level_2_can_have_dark_rooms()
        {
            RegisterRandomNumber("gone rooms", 0);
            RegisterRandomNumber("is dark room", 0);
            RegisterRandomNumber("is maze room", 1);

            rogue.level = 2;
            rogue.new_level();

            rogue.rooms.Count(r => r.IsDark).Should().Be(9);
        }
    }
}
