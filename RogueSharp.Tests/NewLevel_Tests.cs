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
        public void Level_1_never_has_any_dark_rooms_or_maze_rooms()
        {
            RegisterRandomNumber("is dark room", 0);

            rogue.new_level();

            rogue.rooms.Count(r => r.IsDark).Should().Be(0);
            rogue.rooms.Count(r => r.IsMaze).Should().Be(0);
        }

        [Fact]
        public void Level_2_and_up_can_have_dark_rooms()
        {
            RegisterRandomNumber("gone rooms", 0);
            RegisterRandomNumber("is dark room", 0);
            RegisterRandomNumber("is maze room", 1);

            rogue.level = 2;
            rogue.new_level();

            rogue.rooms.Count(r => r.IsDark).Should().Be(9);
        }

        [Fact]
        public void Level_2_and_up_can_have_maze_rooms()
        {
            RegisterRandomNumber("gone rooms", 0);
            RegisterRandomNumber("is dark room", 0);
            RegisterRandomNumbers("is maze room", 0, 1);

            rogue.level = 2;
            rogue.new_level();

            rogue.rooms.Count(r => r.IsMaze).Should().Be(5);
            rogue.rooms.Count(r => !r.IsMaze).Should().Be(4);
        }

        [Fact]
        public void Each_room_is_in_its_proper_section_of_the_screen()
        {
            RegisterRandomNumber("gone rooms", 0);

            rogue.new_level();

            for (int i = 0; i < 9; i++)
                CheckRoom(i);
        }

        private void CheckRoom(int roomNumber)
        {
            // Too much implementation details?
            rogue.rooms[roomNumber].r_pos.x.Should().Be.GreaterThanOrEqualTo((roomNumber % 3) * (Rogue.NUMCOLS / 3) + 1);
            rogue.rooms[roomNumber].r_pos.y.Should().Be.GreaterThanOrEqualTo((roomNumber / 3) * (Rogue.NUMLINES / 3));
            rogue.rooms[roomNumber].r_max.x.Should().Be.GreaterThanOrEqualTo(4);
            rogue.rooms[roomNumber].r_max.y.Should().Be.GreaterThanOrEqualTo(4);
            rogue.rooms[roomNumber].r_max.x.Should().Be.LessThanOrEqualTo(Rogue.NUMCOLS / 3);
            rogue.rooms[roomNumber].r_max.y.Should().Be.LessThanOrEqualTo(Rogue.NUMCOLS / 3);
        }

        [Fact]
        public void Gone_rooms_never_have_gold_in_them()
        {
            RegisterRandomNumber("gone rooms", 3);
            RegisterRandomNumber("has gold", 0);

            rogue.new_level();

            rogue.rooms.Count(r => r.IsGone && r.GoldValue == 0).Should().Be(3);
        }

        [Fact]
        public void Non_gone_rooms_have_a_1_in_2_chance_of_having_gold_in_them()
        {
            RegisterRandomNumber("gone rooms", 0);
            RegisterRandomNumbers("has gold", 0, 1);

            rogue.new_level();

            rogue.rooms.Count(r => r.GoldValue > 0).Should().Be(5);
            rogue.rooms.Count(r => r.GoldValue == 0).Should().Be(4);
        }

        [Fact]
        public void Rooms_with_gold_in_them_have_an_80_percent_chance_of_having_a_montser_in_them()
        {
            RegisterRandomNumber("gone rooms", 0);
            RegisterRandomNumber("has gold", 0);
            RegisterRandomNumbers("has monster", 79, 80);

            rogue.new_level();

            rogue.rooms.Count(MonsterIsInRoom).Should().Be(5);
            rogue.rooms.Count(r => !MonsterIsInRoom(r)).Should().Be(4);
        }

        [Fact]
        public void Rooms_with_no_gold_in_them_have_a_25_percent_chance_of_having_a_montser_in_them()
        {
            RegisterRandomNumber("gone rooms", 0);
            RegisterRandomNumber("has gold", 1);
            RegisterRandomNumbers("has monster", 24, 25);

            rogue.new_level();

            rogue.rooms.Count(MonsterIsInRoom).Should().Be(5);
            rogue.rooms.Count(MonsterIsNotInRoom).Should().Be(4);
        }

        private bool MonsterIsInRoom(Rogue.room room)
        {
            return rogue.MonstersOnLevel.Any(m => m.t_room == room);
        }

        private bool MonsterIsNotInRoom(Rogue.room room)
        {
            return !MonsterIsInRoom(room);
        }
    }
}
