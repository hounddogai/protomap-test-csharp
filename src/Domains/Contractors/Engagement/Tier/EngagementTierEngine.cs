namespace Acme.Workforce.Contractors.Engagement.Tier;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the engagement tier calculation.</summary>
public interface IEngagementTierFactor
{
    string Describe();
    double Adjust(EngagementTier item, double valueValue);
}

public sealed record LabelEngagementTierAdjuster(Dictionary<string, double> Factors) : IEngagementTierFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(EngagementTier item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredEngagementTierAdjuster(List<double> Bounds, List<double> Rates) : IEngagementTierFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(EngagementTier item, double valueValue)
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

public sealed record EngagementTierRevisionOffsetAdjuster(double Weight) : IEngagementTierFactor
{
    public string Describe()
    {
        return $"plus revision times {Weight}";
    }

    public double Adjust(EngagementTier item, double valueValue)
    {
        return valueValue + (double)item.Revision * Weight;
    }
}

public sealed record PercentEngagementTierAdjuster(double Percent) : IEngagementTierFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(EngagementTier item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingEngagementTierAdjuster(double Step) : IEngagementTierFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(EngagementTier item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumEngagementTierAdjuster(double Minimum) : IEngagementTierFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(EngagementTier item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the engagement tier steps in order and keeps a trace of each.</summary>
public sealed class EngagementTierEngine(List<IEngagementTierFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IEngagementTierFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static EngagementTierEngine DefaultEngagementTierEngine()
    {
        return new EngagementTierEngine(
            new List<IEngagementTierFactor>
            {
                new LabelEngagementTierAdjuster(new Dictionary<string, double> { ["west"] = 0.98 }),
                new TieredEngagementTierAdjuster(
                    new List<double> { 5886.0, 8377.0 },
                    new List<double> { 0.84, 0.87 }),
                new EngagementTierRevisionOffsetAdjuster(0.28),
                new PercentEngagementTierAdjuster(-5.3),
                new RoundingEngagementTierAdjuster(0.25),
                new MinimumEngagementTierAdjuster(7.1),
            },
            new List<string>(),
            2.7,
            63856.4);
    }

    public double Evaluate(EngagementTier item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<EngagementTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
