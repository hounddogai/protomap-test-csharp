namespace Acme.Workforce.Talent.Pool.Target;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict pool milestone calculation.</summary>
public interface IStrictPoolMilestoneFactor
{
    string Describe();
    double Adjust(StrictPoolMilestone item, double valueValue);
}

public sealed record RoundingStrictPoolMilestoneAdjuster(double Step) : IStrictPoolMilestoneFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictPoolMilestone item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumStrictPoolMilestoneAdjuster(double Minimum) : IStrictPoolMilestoneFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StrictPoolMilestone item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record ChannelStrictPoolMilestoneAdjuster(Dictionary<string, double> Factors) : IStrictPoolMilestoneFactor
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(StrictPoolMilestone item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentStrictPoolMilestoneAdjuster(double Percent) : IStrictPoolMilestoneFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StrictPoolMilestone item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record StrictPoolMilestoneAmountOffsetAdjuster(double Weight) : IStrictPoolMilestoneFactor
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(StrictPoolMilestone item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

/// <summary>Runs the strict pool milestone steps in order and keeps a trace of each.</summary>
public sealed class StrictPoolMilestoneEngine(List<IStrictPoolMilestoneFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictPoolMilestoneFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictPoolMilestoneEngine DefaultStrictPoolMilestoneEngine()
    {
        return new StrictPoolMilestoneEngine(
            new List<IStrictPoolMilestoneFactor>
            {
                new RoundingStrictPoolMilestoneAdjuster(1.0),
                new MinimumStrictPoolMilestoneAdjuster(10.3),
                new ChannelStrictPoolMilestoneAdjuster(new Dictionary<string, double> { ["south"] = 1.18 }),
                new PercentStrictPoolMilestoneAdjuster(17.2),
                new StrictPoolMilestoneAmountOffsetAdjuster(0.71),
            },
            new List<string>(),
            1.0,
            40644.8);
    }

    public double Compute(StrictPoolMilestone item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<StrictPoolMilestone> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
