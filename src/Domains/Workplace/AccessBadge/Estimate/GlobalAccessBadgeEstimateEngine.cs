namespace Acme.Workforce.Workplace.AccessBadge.Estimate;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the global access badge estimate calculation.</summary>
public interface IGlobalAccessBadgeEstimateModifier
{
    string Describe();
    double Adjust(GlobalAccessBadgeEstimate item, double valueValue);
}

public sealed record GlobalAccessBadgeEstimateCapacityOffsetAdjuster(double Weight) : IGlobalAccessBadgeEstimateModifier
{
    public string Describe()
    {
        return $"plus capacity times {Weight}";
    }

    public double Adjust(GlobalAccessBadgeEstimate item, double valueValue)
    {
        return valueValue + (double)item.Capacity * Weight;
    }
}

public sealed record FlaggedGlobalAccessBadgeEstimateAdjuster(Registry Flags, string Flag, double Percent) : IGlobalAccessBadgeEstimateModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(GlobalAccessBadgeEstimate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumGlobalAccessBadgeEstimateAdjuster(double Minimum) : IGlobalAccessBadgeEstimateModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GlobalAccessBadgeEstimate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the global access badge estimate steps in order and keeps a trace of each.</summary>
public sealed class GlobalAccessBadgeEstimateEngine(List<IGlobalAccessBadgeEstimateModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalAccessBadgeEstimateModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalAccessBadgeEstimateEngine DefaultGlobalAccessBadgeEstimateEngine(Registry flags)
    {
        return new GlobalAccessBadgeEstimateEngine(
            new List<IGlobalAccessBadgeEstimateModifier>
            {
                new GlobalAccessBadgeEstimateCapacityOffsetAdjuster(1.34),
                new FlaggedGlobalAccessBadgeEstimateAdjuster(flags, "global-access-badge-estimate-manual", 4.5),
                new MinimumGlobalAccessBadgeEstimateAdjuster(32.6),
            },
            new List<string>(),
            3.3,
            17361.0);
    }

    public double Price(GlobalAccessBadgeEstimate item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalAccessBadgeEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
