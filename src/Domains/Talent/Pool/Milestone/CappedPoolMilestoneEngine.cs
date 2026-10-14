namespace Acme.Workforce.Talent.Pool.Milestone;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped pool milestone calculation.</summary>
public interface ICappedPoolMilestoneAdjuster
{
    string Describe();
    double Adjust(CappedPoolMilestone item, double valueValue);
}

public sealed record RoundingCappedPoolMilestoneAdjuster(double Step) : ICappedPoolMilestoneAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedPoolMilestone item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredCappedPoolMilestoneAdjuster(List<double> Bounds, List<double> Rates) : ICappedPoolMilestoneAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedPoolMilestone item, double valueValue)
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

public sealed record MinimumCappedPoolMilestoneAdjuster(double Minimum) : ICappedPoolMilestoneAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CappedPoolMilestone item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentCappedPoolMilestoneAdjuster(double Percent) : ICappedPoolMilestoneAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CappedPoolMilestone item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapCappedPoolMilestoneAdjuster(double Cap) : ICappedPoolMilestoneAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CappedPoolMilestone item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record CappedPoolMilestoneVersionOffsetAdjuster(double Weight) : ICappedPoolMilestoneAdjuster
{
    public string Describe()
    {
        return $"plus version times {Weight}";
    }

    public double Adjust(CappedPoolMilestone item, double valueValue)
    {
        return valueValue + (double)item.Version * Weight;
    }
}

/// <summary>Runs the capped pool milestone steps in order and keeps a trace of each.</summary>
public sealed class CappedPoolMilestoneEngine(List<ICappedPoolMilestoneAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedPoolMilestoneAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedPoolMilestoneEngine DefaultCappedPoolMilestoneEngine()
    {
        return new CappedPoolMilestoneEngine(
            new List<ICappedPoolMilestoneAdjuster>
            {
                new RoundingCappedPoolMilestoneAdjuster(0.1),
                new TieredCappedPoolMilestoneAdjuster(
                    new List<double> { 1529.0, 2148.0, 4932.0, 6954.0 },
                    new List<double> { 0.96, 1.03, 1.05, 1.09 }),
                new MinimumCappedPoolMilestoneAdjuster(29.7),
                new PercentCappedPoolMilestoneAdjuster(-18.8),
                new CapCappedPoolMilestoneAdjuster(26692.0),
                new CappedPoolMilestoneVersionOffsetAdjuster(1.87),
            },
            new List<string>(),
            1.8,
            14401.4);
    }

    public double Price(CappedPoolMilestone item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<CappedPoolMilestone> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
