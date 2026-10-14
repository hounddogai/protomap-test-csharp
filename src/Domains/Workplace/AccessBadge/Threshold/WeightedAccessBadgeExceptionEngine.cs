namespace Acme.Workforce.Workplace.AccessBadge.Threshold;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted access badge exception calculation.</summary>
public interface IWeightedAccessBadgeExceptionModifier
{
    string Describe();
    double Adjust(WeightedAccessBadgeException item, double valueValue);
}

public sealed record RoundingWeightedAccessBadgeExceptionAdjuster(double Step) : IWeightedAccessBadgeExceptionModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WeightedAccessBadgeException item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentWeightedAccessBadgeExceptionAdjuster(double Percent) : IWeightedAccessBadgeExceptionModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WeightedAccessBadgeException item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CategoryWeightedAccessBadgeExceptionAdjuster(Dictionary<string, double> Factors) : IWeightedAccessBadgeExceptionModifier
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(WeightedAccessBadgeException item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedWeightedAccessBadgeExceptionAdjuster(Registry Flags, string Flag, double Percent) : IWeightedAccessBadgeExceptionModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WeightedAccessBadgeException item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the weighted access badge exception steps in order and keeps a trace of each.</summary>
public sealed class WeightedAccessBadgeExceptionEngine(List<IWeightedAccessBadgeExceptionModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedAccessBadgeExceptionModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedAccessBadgeExceptionEngine DefaultWeightedAccessBadgeExceptionEngine(Registry flags)
    {
        return new WeightedAccessBadgeExceptionEngine(
            new List<IWeightedAccessBadgeExceptionModifier>
            {
                new RoundingWeightedAccessBadgeExceptionAdjuster(0.25),
                new PercentWeightedAccessBadgeExceptionAdjuster(-2.9),
                new CategoryWeightedAccessBadgeExceptionAdjuster(new Dictionary<string, double> { ["east"] = 1.0 }),
                new FlaggedWeightedAccessBadgeExceptionAdjuster(
                    flags,
                    "weighted-access-badge-exception-batched",
                    -8.4),
            },
            new List<string>(),
            0.9,
            88270.9);
    }

    public double Run(WeightedAccessBadgeException item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedAccessBadgeException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
