namespace Acme.Workforce.Contractors.Rate.Cycle;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the rate cycle calculation.</summary>
public interface IRateCycleModifier
{
    string Describe();
    double Adjust(RateCycle item, double valueValue);
}

public sealed record FlaggedRateCycleAdjuster(Registry Flags, string Flag, double Percent) : IRateCycleModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RateCycle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentRateCycleAdjuster(double Percent) : IRateCycleModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RateCycle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RateCycleStartsAtOffsetAdjuster(double Weight) : IRateCycleModifier
{
    public string Describe()
    {
        return $"plus starts at times {Weight}";
    }

    public double Adjust(RateCycle item, double valueValue)
    {
        return valueValue + (double)item.StartsAt * Weight;
    }
}

/// <summary>Runs the rate cycle steps in order and keeps a trace of each.</summary>
public sealed class RateCycleEngine(List<IRateCycleModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRateCycleModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RateCycleEngine DefaultRateCycleEngine(Registry flags)
    {
        return new RateCycleEngine(
            new List<IRateCycleModifier>
            {
                new FlaggedRateCycleAdjuster(flags, "rate-cycle-seasonal", -6.0),
                new PercentRateCycleAdjuster(-9.3),
                new RateCycleStartsAtOffsetAdjuster(1.04),
            },
            new List<string>(),
            0.2,
            21699.2);
    }

    public double Compute(RateCycle item)
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

    public Dictionary<string, double> EvaluateAll(List<RateCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
