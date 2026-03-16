using BayWyn_Couriers.ViewModels;
using System.Security.Cryptography;

namespace BayWyn_Couriers_UnitTest
{
    public class UnitTest1
    {
        [Fact]
        public void CountSlots_Return32()
        {
            //Arrange
            var vm = new AdminVM(null);

            //Act
                
            //Assert
            Assert.Equal(32, vm.SlotsDictionary.Count);

        }

        [Fact]
        public void GetValueOfS1_Return_8_30() {
            //Arrange
            var vm = new AdminVM(null);

            //Assert
            Assert.Equal("08:30", vm.SlotsDictionary["S1"]);
        }

        [Fact]
        public void GetValueOfS32_Return_16_30()
        {
            //Arrange
            var vm = new AdminVM(null);
            
            //Assert
            Assert.Equal("16:15", vm.SlotsDictionary["S32"]);
        }

        [Fact]
        public void GetDifferenceOfSlots_Return30()
        {
            //Arrange
            var vm = new AdminVM(null);

            //Act
            DateTime time1 = DateTime.Parse(vm.SlotsDictionary["S1"]);
            DateTime time2 = DateTime.Parse(vm.SlotsDictionary["S2"]);

            // Calculate the difference
            TimeSpan difference = time2 - time1;

            // Assert: Check if the difference is 15 minutes (TotalMinutes)
            Assert.Equal(15, difference.Minutes);
        }


        [Fact]
        public void RefreshAvailableSlots_DisableType1Slots_ForEvenCouriers()
        {
            //Arrange
            var vm = new LCVM(null);

            //Act (sending an even id)
            vm.RefreshAvailableSlots(DateTime.Today, 2);

            //Assert
            // Check one from Type 1 (should be disabled)
            Assert.False(vm.TimeSlots.First(s => s.SlotName == "S15").IsEnabled);
            // Check one from Type 2 (should still be enabled)
            Assert.True(vm.TimeSlots.First(s => s.SlotName == "S19").IsEnabled);
        }

        [Fact]
        public void RefreshAvailableSlots_DisableType2Slots_ForOddCouriers()
        {
            //Arrange
            var vm = new LCVM(null);

            //Act (Sending an odd id)
            vm.RefreshAvailableSlots(DateTime.Today, 3);

            // Assert
            // Check one from Type 2 (should be disabled)
            Assert.False(vm.TimeSlots.First(s => s.SlotName == "S19").IsEnabled);
            // Check one from Type 1 (should still be enabled)
            Assert.True(vm.TimeSlots.First(s => s.SlotName == "S15").IsEnabled);
        }

        [Fact]
        public void RefreshAvailableSlots_DisabledSlotsMessageForEvenID_Returns_BreakDisabled()
        {
            //Arrange
            var vm = new LCVM(null);
            //Act
            vm.RefreshAvailableSlots(DateTime.Today, 2); // Even courier disables S15
            //Assert
            var slot = vm.TimeSlots.First(s => s.SlotName == "S15");
            Assert.Equal("Break/Disabled", slot.DisplayName);
        }

        [Fact]
        public void RefreshAvailableSlots_DisabledSlotsMessageForOddID_Returns_BreakDisabled()
        {
            //Arrange
            var vm = new LCVM(null);
            //Act
            vm.RefreshAvailableSlots(DateTime.Today, 3); // Even courier disables S15
            //Assert
            var slot = vm.TimeSlots.First(s => s.SlotName == "S19");
            Assert.Equal("Break/Disabled", slot.DisplayName);
        }

        [Fact]
        public void RefreshAvailableSlots_PreviousValuesCleared_ReturnsTrue()
        {
            //Arrange
            var vm = new LCVM(null);
            //Act
            // Manually disabling S1 (to check if it resets)
            vm.TimeSlots.First(s => s.SlotName == "S1").IsEnabled = false;
            vm.RefreshAvailableSlots(DateTime.Today, 2);

            //Assert
            // S1 should now be enabled again because of the reset loop
            Assert.True(vm.TimeSlots.First(s => s.SlotName == "S1").IsEnabled);
        }

    }
}