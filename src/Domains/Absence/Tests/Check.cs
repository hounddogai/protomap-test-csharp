namespace Acme.Workforce.Absence.Tests;

/// <summary>Assertions of the checks, which throw on the first failure.</summary>
public static class Check
{
    public static void Equal<T>(T actual, T expected, string what)
    {
        if (!EqualityComparer<T>.Default.Equals(actual, expected))
            throw new InvalidOperationException($"{what}: got {actual}, want {expected}");
    }

    public static void True(bool condition, string what)
    {
        if (!condition)
            throw new InvalidOperationException($"unexpected {what}");
    }

    public static void Fails(Action action, string what)
    {
        try
        {
            action();
        }
        catch (InvalidOperationException)
        {
            return;
        }
        throw new InvalidOperationException($"expected {what} to fail");
    }
}
