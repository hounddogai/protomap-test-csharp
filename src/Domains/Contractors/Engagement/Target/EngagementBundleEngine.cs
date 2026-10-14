namespace Acme.Workforce.Contractors.Engagement.Target;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the engagement bundle calculation.</summary>
public interface IEngagementBundleFactor
{
    string Describe();
    double Adjust(EngagementBundle item, double valueValue);
}

public sealed record RoundingEngagementBundleAdjuster(double Step) : IEngagementBundleFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(EngagementBundle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedEngagementBundleAdjuster(Registry Flags, string Flag, double Percent) : IEngagementBundleFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(EngagementBundle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumEngagementBundleAdjuster(double Minimum) : IEngagementBundleFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(EngagementBundle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentEngagementBundleAdjuster(double Percent) : IEngagementBundleFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(EngagementBundle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CountryEngagementBundleAdjuster(Dictionary<string, double> Factors) : IEngagementBundleFactor
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(EngagementBundle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the engagement bundle steps in order and keeps a trace of each.</summary>
public sealed class EngagementBundleEngine(List<IEngagementBundleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IEngagementBundleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static EngagementBundleEngine DefaultEngagementBundleEngine(Registry flags)
    {
        return new EngagementBundleEngine(
            new List<IEngagementBundleFactor>
            {
                new RoundingEngagementBundleAdjuster(0.25),
                new FlaggedEngagementBundleAdjuster(flags, "engagement-bundle-capped", -1.8),
                new MinimumEngagementBundleAdjuster(27.5),
                new PercentEngagementBundleAdjuster(-21.6),
                new CountryEngagementBundleAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.13, ["online"] = 1.24, ["north"] = 1.04 }),
            },
            new List<string>(),
            3.7,
            65697.3);
    }

    public double Price(EngagementBundle item)
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

    public Dictionary<string, double> EvaluateAll(List<EngagementBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
