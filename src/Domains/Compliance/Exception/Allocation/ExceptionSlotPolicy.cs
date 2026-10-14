namespace Acme.Workforce.Compliance.Exception.Allocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of exception slot policy.</summary>
public static class ExceptionSlotPolicy
{
    /// <summary>Puts a exception slot code in its canonical ES- form.</summary>
    public static string NormalizeExceptionSlotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ES-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ES-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string ExceptionSlotBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
