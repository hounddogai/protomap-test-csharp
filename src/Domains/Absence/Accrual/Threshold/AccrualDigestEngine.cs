namespace Acme.Workforce.Absence.Accrual.Threshold;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the accrual digest calculation.</summary>
public interface IAccrualDigestStep
{
    string Describe();
    double Adjust(AccrualDigest item, double valueValue);
}

public sealed record FlaggedAccrualDigestAdjuster(Registry Flags, string Flag, double Percent) : IAccrualDigestStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AccrualDigest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SourceAccrualDigestAdjuster(Dictionary<string, double> Factors) : IAccrualDigestStep
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(AccrualDigest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredAccrualDigestAdjuster(List<double> Bounds, List<double> Rates) : IAccrualDigestStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AccrualDigest item, double valueValue)
    {
        for (var index = 0; index < Bounds.Count; index++)
        {
            if (valueValue <= Bounds[index] && index < Rates.Count)
            {
                return valueValue * Rates[index];
            }
        }
        return valueValue;
    }
}

public sealed record CapAccrualDigestAdjuster(double Cap) : IAccrualDigestStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AccrualDigest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record AccrualDigestQuantityOffsetAdjuster(double Weight) : IAccrualDigestStep
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(AccrualDigest item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

/// <summary>Runs the accrual digest steps in order and keeps a trace of each.</summary>
public sealed class AccrualDigestEngine(List<IAccrualDigestStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccrualDigestStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccrualDigestEngine DefaultAccrualDigestEngine(Registry flags)
    {
        return new AccrualDigestEngine(
            new List<IAccrualDigestStep>
            {
                new FlaggedAccrualDigestAdjuster(flags, "accrual-digest-adaptive", 3.7),
                new SourceAccrualDigestAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.28, ["online"] = 1.1, ["east"] = 1.36 }),
                new TieredAccrualDigestAdjuster(new List<double> { 509.0, 7580.0 }, new List<double> { 0.89, 1.19 }),
                new CapAccrualDigestAdjuster(21235.0),
                new AccrualDigestQuantityOffsetAdjuster(1.24),
            },
            new List<string>(),
            2.6,
            67937.0);
    }

    public double Compute(AccrualDigest item)
    {
        var current = item.AdjustedValue();
        Trace = new List<string>();
        foreach (var step in Steps)
        {
            var next = step.Adjust(item, current);
            Trace.Add($"{step.Describe()}: {Math.Round(current, 2)} -> {Math.Round(next, 2)}");
            current = next;
        }
        return Numbers.ClampFloat(current, Floor, Ceiling);
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<AccrualDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
