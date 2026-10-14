namespace Acme.Workforce.Compensation.Benchmark.Milestone;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive benchmark milestone calculation.</summary>
public interface IAdaptiveBenchmarkMilestoneFactor
{
    string Describe();
    double Adjust(AdaptiveBenchmarkMilestone item, double valueValue);
}

public sealed record AdaptiveBenchmarkMilestoneExpiresAtOffsetAdjuster(double Weight) : IAdaptiveBenchmarkMilestoneFactor
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(AdaptiveBenchmarkMilestone item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

public sealed record MinimumAdaptiveBenchmarkMilestoneAdjuster(double Minimum) : IAdaptiveBenchmarkMilestoneFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptiveBenchmarkMilestone item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredAdaptiveBenchmarkMilestoneAdjuster(List<double> Bounds, List<double> Rates) : IAdaptiveBenchmarkMilestoneFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdaptiveBenchmarkMilestone item, double valueValue)
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

public sealed record RoundingAdaptiveBenchmarkMilestoneAdjuster(double Step) : IAdaptiveBenchmarkMilestoneFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptiveBenchmarkMilestone item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapAdaptiveBenchmarkMilestoneAdjuster(double Cap) : IAdaptiveBenchmarkMilestoneFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptiveBenchmarkMilestone item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record NameAdaptiveBenchmarkMilestoneAdjuster(Dictionary<string, double> Factors) : IAdaptiveBenchmarkMilestoneFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(AdaptiveBenchmarkMilestone item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the adaptive benchmark milestone steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveBenchmarkMilestoneEngine(List<IAdaptiveBenchmarkMilestoneFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveBenchmarkMilestoneFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveBenchmarkMilestoneEngine DefaultAdaptiveBenchmarkMilestoneEngine()
    {
        return new AdaptiveBenchmarkMilestoneEngine(
            new List<IAdaptiveBenchmarkMilestoneFactor>
            {
                new AdaptiveBenchmarkMilestoneExpiresAtOffsetAdjuster(1.65),
                new MinimumAdaptiveBenchmarkMilestoneAdjuster(46.4),
                new TieredAdaptiveBenchmarkMilestoneAdjuster(
                    new List<double> { 5719.0, 6468.0, 6664.0, 9035.0 },
                    new List<double> { 0.87, 0.9, 1.0, 1.06 }),
                new RoundingAdaptiveBenchmarkMilestoneAdjuster(0.05),
                new CapAdaptiveBenchmarkMilestoneAdjuster(17041.0),
                new NameAdaptiveBenchmarkMilestoneAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.96, ["south"] = 1.37, ["central"] = 1.03 }),
            },
            new List<string>(),
            2.7,
            65834.3);
    }

    public double Compute(AdaptiveBenchmarkMilestone item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveBenchmarkMilestone> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
