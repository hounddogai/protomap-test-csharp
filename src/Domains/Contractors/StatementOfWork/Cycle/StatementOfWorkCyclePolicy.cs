namespace Acme.Workforce.Contractors.StatementOfWork.Cycle;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of statement of work cycle policy.</summary>
public static class StatementOfWorkCyclePolicy
{
    public static int StatementOfWorkCycleSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 60;
    }

    public static string StatementOfWorkCycleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Puts a statement of work cycle code in its canonical SOWC- form.</summary>
    public static string NormalizeStatementOfWorkCycleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SOWC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SOWC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double StatementOfWorkCycleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.75;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
