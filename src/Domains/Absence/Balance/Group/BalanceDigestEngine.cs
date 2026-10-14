namespace Acme.Workforce.Absence.Balance.Group;

/// <summary>One step of the balance digest calculation.</summary>
public interface IBalanceDigestFactor
{
    string Describe();
    double Adjust(BalanceDigest item, double valueValue);
}

public sealed record CapBalanceDigestAdjuster(double Cap) : IBalanceDigestFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BalanceDigest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentBalanceDigestAdjuster(double Percent) : IBalanceDigestFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BalanceDigest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SegmentBalanceDigestAdjuster(Dictionary<string, double> Factors) : IBalanceDigestFactor
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(BalanceDigest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the balance digest steps in order and keeps a trace of each.</summary>
public sealed class BalanceDigestEngine(List<IBalanceDigestFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBalanceDigestFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BalanceDigestEngine DefaultBalanceDigestEngine()
    {
        return new BalanceDigestEngine(
            new List<IBalanceDigestFactor>
            {
                new CapBalanceDigestAdjuster(26925.0),
                new PercentBalanceDigestAdjuster(-20.5),
                new SegmentBalanceDigestAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.74, ["retail"] = 0.94, ["north"] = 0.72 }),
            },
            new List<string>(),
            3.0,
            89875.8);
    }

    public double Price(BalanceDigest item)
    {
        var current = item.ProjectedVolume();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<BalanceDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
