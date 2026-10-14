namespace Acme.Workforce.Contractors.Rate.Cycle;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the default rate cycle calculation.</summary>
public interface IDefaultRateCycleModifier
{
    string Describe();
    double Adjust(DefaultRateCycle item, double valueValue);
}

public sealed record RegionDefaultRateCycleAdjuster(Dictionary<string, double> Factors) : IDefaultRateCycleModifier
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(DefaultRateCycle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record DefaultRateCycleEndsAtOffsetAdjuster(double Weight) : IDefaultRateCycleModifier
{
    public string Describe()
    {
        return $"plus ends at times {Weight}";
    }

    public double Adjust(DefaultRateCycle item, double valueValue)
    {
        return valueValue + (double)item.EndsAt * Weight;
    }
}

public sealed record CapDefaultRateCycleAdjuster(double Cap) : IDefaultRateCycleModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultRateCycle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedDefaultRateCycleAdjuster(Registry Flags, string Flag, double Percent) : IDefaultRateCycleModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultRateCycle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the default rate cycle steps in order and keeps a trace of each.</summary>
public sealed class DefaultRateCycleEngineExtra(List<IDefaultRateCycleModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultRateCycleModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public double Price(DefaultRateCycle item)
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

    public Dictionary<string, double> EvaluateAll(List<DefaultRateCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}

/// <summary>Functions of default rate cycle engine.</summary>
public static class DefaultRateCycleEngine
{
    public static DefaultRateCycleEngineExtra DefaultDefaultRateCycleEngine(Registry flags)
    {
        return new DefaultRateCycleEngineExtra(
            new List<IDefaultRateCycleModifier>
            {
                new RegionDefaultRateCycleAdjuster(new Dictionary<string, double> { ["central"] = 0.71 }),
                new DefaultRateCycleEndsAtOffsetAdjuster(0.4),
                new CapDefaultRateCycleAdjuster(13040.0),
                new FlaggedDefaultRateCycleAdjuster(flags, "default-rate-cycle-legacy", 9.1),
            },
            new List<string>(),
            4.6,
            13768.5);
    }
}
