namespace Acme.Workforce.Contractors.Rate.RateCycle;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the weighted rate cycle calculation.</summary>
public interface IWeightedRateCycleFactor
{
    string Describe();
    double Adjust(WeightedRateCycle item, double valueValue);
}

public sealed record FlaggedWeightedRateCycleAdjuster(Registry Flags, string Flag, double Percent) : IWeightedRateCycleFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WeightedRateCycle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentWeightedRateCycleAdjuster(double Percent) : IWeightedRateCycleFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WeightedRateCycle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapWeightedRateCycleAdjuster(double Cap) : IWeightedRateCycleFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WeightedRateCycle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TeamWeightedRateCycleAdjuster(Dictionary<string, double> Factors) : IWeightedRateCycleFactor
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(WeightedRateCycle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the weighted rate cycle steps in order and keeps a trace of each.</summary>
public sealed class WeightedRateCycleEngine(List<IWeightedRateCycleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedRateCycleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedRateCycleEngine DefaultWeightedRateCycleEngine(Registry flags)
    {
        return new WeightedRateCycleEngine(
            new List<IWeightedRateCycleFactor>
            {
                new FlaggedWeightedRateCycleAdjuster(flags, "weighted-rate-cycle-pending", 7.3),
                new PercentWeightedRateCycleAdjuster(19.4),
                new CapWeightedRateCycleAdjuster(25336.0),
                new TeamWeightedRateCycleAdjuster(new Dictionary<string, double> { ["west"] = 1.34 }),
            },
            new List<string>(),
            0.3,
            17001.1);
    }

    public double Compute(WeightedRateCycle item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedRateCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
