namespace Acme.Workforce.Orgdesign.Headcount.Group;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the adaptive headcount group calculation.</summary>
public interface IAdaptiveHeadcountGroupModifier
{
    string Describe();
    double Adjust(AdaptiveHeadcountGroup item, double valueValue);
}

public sealed record LocaleAdaptiveHeadcountGroupAdjuster(Dictionary<string, double> Factors) : IAdaptiveHeadcountGroupModifier
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(AdaptiveHeadcountGroup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapAdaptiveHeadcountGroupAdjuster(double Cap) : IAdaptiveHeadcountGroupModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptiveHeadcountGroup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedAdaptiveHeadcountGroupAdjuster(Registry Flags, string Flag, double Percent) : IAdaptiveHeadcountGroupModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdaptiveHeadcountGroup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentAdaptiveHeadcountGroupAdjuster(double Percent) : IAdaptiveHeadcountGroupModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdaptiveHeadcountGroup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the adaptive headcount group steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveHeadcountGroupEngine(List<IAdaptiveHeadcountGroupModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveHeadcountGroupModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveHeadcountGroupEngine DefaultAdaptiveHeadcountGroupEngine(Registry flags)
    {
        return new AdaptiveHeadcountGroupEngine(
            new List<IAdaptiveHeadcountGroupModifier>
            {
                new LocaleAdaptiveHeadcountGroupAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.17, ["north"] = 1.24 }),
                new CapAdaptiveHeadcountGroupAdjuster(36805.0),
                new FlaggedAdaptiveHeadcountGroupAdjuster(flags, "adaptive-headcount-group-global", -1.1),
                new PercentAdaptiveHeadcountGroupAdjuster(-21.7),
            },
            new List<string>(),
            0.6,
            3734.1);
    }

    public double Price(AdaptiveHeadcountGroup item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveHeadcountGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
