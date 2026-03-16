namespace DBTestApp;

public class Calculator
{
    public int Add(int a, int b) => a + b;

    public int Subtract(int a, int b) => a - b;

    public int Multiply(int a, int b) => a * b;

    public int Divide(int a, int b)
    {
        if (b == 0) throw new ArgumentException("Cannot divide by zero.");
        return a / b;
    }

    public bool IsEven(int number) => number % 2 == 0;
}

public class Program
{
    public static void Main()
    {
        Calculator calculator = new Calculator();

        var x = calculator.Add(1, 2);
        Console.WriteLine($"1+2 = {x}");

        var y = calculator.Divide(9, 3);
        Console.WriteLine($"1 / 3 = {y}");

        Console.WriteLine($"Is var x (from the addition above) an even number: {calculator.IsEven(x)}");
    }
}