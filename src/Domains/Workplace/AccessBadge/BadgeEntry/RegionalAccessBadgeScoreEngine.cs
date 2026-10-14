namespace Acme.Workforce.Workplace.AccessBadge.BadgeEntry;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional access badge score calculation.</summary>
public interface IRegionalAccessBadgeScoreFactor
{
    string Describe();
    double Adjust(RegionalAccessBadgeScore item, double valueValue);
}

public sealed record TieredRegionalAccessBadgeScoreAdjuster(List<double> Bounds, List<double> Rates) : IRegionalAccessBadgeScoreFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RegionalAccessBadgeScore item, double valueValue)
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

public sealed record MinimumRegionalAccessBadgeScoreAdjuster(double Minimum) : IRegionalAccessBadgeScoreFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RegionalAccessBadgeScore item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentRegionalAccessBadgeScoreAdjuster(double Percent) : IRegionalAccessBadgeScoreFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalAccessBadgeScore item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NameRegionalAccessBadgeScoreAdjuster(Dictionary<string, double> Factors) : IRegionalAccessBadgeScoreFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(RegionalAccessBadgeScore item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapRegionalAccessBadgeScoreAdjuster(double Cap) : IRegionalAccessBadgeScoreFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RegionalAccessBadgeScore item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the regional access badge score steps in order and keeps a trace of each.</summary>
public sealed class RegionalAccessBadgeScoreEngine(List<IRegionalAccessBadgeScoreFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalAccessBadgeScoreFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalAccessBadgeScoreEngine DefaultRegionalAccessBadgeScoreEngine()
    {
        return new RegionalAccessBadgeScoreEngine(
            new List<IRegionalAccessBadgeScoreFactor>
            {
                new TieredRegionalAccessBadgeScoreAdjuster(
                    new List<double> { 563.0, 1636.0, 3194.0, 3194.0 },
                    new List<double> { 1.01, 1.17, 0.96, 1.18 }),
                new MinimumRegionalAccessBadgeScoreAdjuster(11.9),
                new PercentRegionalAccessBadgeScoreAdjuster(23.8),
                new NameRegionalAccessBadgeScoreAdjuster(new Dictionary<string, double> { ["north"] = 0.88 }),
                new CapRegionalAccessBadgeScoreAdjuster(26126.0),
            },
            new List<string>(),
            4.3,
            76225.8);
    }

    public double Compute(RegionalAccessBadgeScore item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalAccessBadgeScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
