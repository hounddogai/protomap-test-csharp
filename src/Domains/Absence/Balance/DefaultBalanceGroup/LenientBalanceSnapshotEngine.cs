namespace Acme.Workforce.Absence.Balance.DefaultBalanceGroup;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient balance snapshot calculation.</summary>
public interface ILenientBalanceSnapshotAdjuster
{
    string Describe();
    double Adjust(LenientBalanceSnapshot item, double valueValue);
}

public sealed record SourceLenientBalanceSnapshotAdjuster(Dictionary<string, double> Factors) : ILenientBalanceSnapshotAdjuster
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(LenientBalanceSnapshot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record LenientBalanceSnapshotStartsAtOffsetAdjuster(double Weight) : ILenientBalanceSnapshotAdjuster
{
    public string Describe()
    {
        return $"plus starts at times {Weight}";
    }

    public double Adjust(LenientBalanceSnapshot item, double valueValue)
    {
        return valueValue + (double)item.StartsAt * Weight;
    }
}

public sealed record MinimumLenientBalanceSnapshotAdjuster(double Minimum) : ILenientBalanceSnapshotAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientBalanceSnapshot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentLenientBalanceSnapshotAdjuster(double Percent) : ILenientBalanceSnapshotAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientBalanceSnapshot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingLenientBalanceSnapshotAdjuster(double Step) : ILenientBalanceSnapshotAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientBalanceSnapshot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredLenientBalanceSnapshotAdjuster(List<double> Bounds, List<double> Rates) : ILenientBalanceSnapshotAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientBalanceSnapshot item, double valueValue)
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

/// <summary>Runs the lenient balance snapshot steps in order and keeps a trace of each.</summary>
public sealed class LenientBalanceSnapshotEngine(List<ILenientBalanceSnapshotAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientBalanceSnapshotAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientBalanceSnapshotEngine DefaultLenientBalanceSnapshotEngine()
    {
        return new LenientBalanceSnapshotEngine(
            new List<ILenientBalanceSnapshotAdjuster>
            {
                new SourceLenientBalanceSnapshotAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.73, ["west"] = 1.01, ["retail"] = 0.87 }),
                new LenientBalanceSnapshotStartsAtOffsetAdjuster(0.83),
                new MinimumLenientBalanceSnapshotAdjuster(11.6),
                new PercentLenientBalanceSnapshotAdjuster(3.5),
                new RoundingLenientBalanceSnapshotAdjuster(1.0),
                new TieredLenientBalanceSnapshotAdjuster(
                    new List<double> { 930.0, 1184.0, 4943.0, 9568.0 },
                    new List<double> { 0.93, 0.98, 0.97, 1.13 }),
            },
            new List<string>(),
            0.6,
            94754.4);
    }

    public double Compute(LenientBalanceSnapshot item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<LenientBalanceSnapshot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
