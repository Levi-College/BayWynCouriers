namespace DBTestApp.Test
{
    public class UnitTest1
    {
        [Theory]
        [InlineData(2, 3, 5)]
        [InlineData(0, 0, 0)]
        [InlineData(-1, 5, 4)]
        public void Add_TwoNumbers_ReturnsSum(int a, int b, int expected)
        {
            // Arrange 
            var calc = new Calculator();

            // Act
            var result = calc.Add(a, b);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void IsEven_WhenNumberIsEven_ReturnsTrue()
        {
            // Arrange, Act, Assert
            var calc = new Calculator();
            var result = calc.IsEven(10);
            Assert.True(result);
        }

        [Fact]
        public void IsEven_WhenNumberIsOdd_ReturnsFalse()
        {
            // AAA
            var calc = new Calculator();
            var result = calc.IsEven(11);
            Assert.False(result);
        }

        [Fact]
        public void Divide_ByZero_ThrowsArgumentException()
        {
            var calc = new Calculator();
            var ex = Assert.Throws<ArgumentException>(() => calc.Divide(10, 0));

            Assert.Contains("Cannot divide by zero.", ex.Message);
        }

        [Fact]
        public void Divide_TwoNumbers_ReturnsQoutient()
        {
            var calc = new Calculator();
            var result = calc.Divide(10, 2);

            Assert.Equal(5, result);


        }

    }
}
