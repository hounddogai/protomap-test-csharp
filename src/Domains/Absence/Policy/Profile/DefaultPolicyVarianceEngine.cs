namespace Acme.Workforce.Absence.Policy.Profile;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default policy variance calculation.</summary>
public interface IDefaultPolicyVarianceFactor
{
    string Describe();
    double Adjust(DefaultPolicyVariance item, double valueValue);
}

public sealed record RoundingDefaultPolicyVarianceAdjuster(double Step) : IDefaultPolicyVarianceFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultPolicyVariance item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentDefaultPolicyVarianceAdjuster(double Percent) : IDefaultPolicyVarianceFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultPolicyVariance item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumDefaultPolicyVarianceAdjuster(double Minimum) : IDefaultPolicyVarianceFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultPolicyVariance item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record SegmentDefaultPolicyVarianceAdjuster(Dictionary<string, double> Factors) : IDefaultPolicyVarianceFactor
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(DefaultPolicyVariance item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedDefaultPolicyVarianceAdjuster(Registry Flags, string Flag, double Percent) : IDefaultPolicyVarianceFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultPolicyVariance item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the default policy variance steps in order and keeps a trace of each.</summary>
public sealed class DefaultPolicyVarianceEngine(List<IDefaultPolicyVarianceFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultPolicyVarianceFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultPolicyVarianceEngine DefaultDefaultPolicyVarianceEngine(Registry flags)
    {
        return new DefaultPolicyVarianceEngine(
            new List<IDefaultPolicyVarianceFactor>
            {
                new RoundingDefaultPolicyVarianceAdjuster(5.0),
                new PercentDefaultPolicyVarianceAdjuster(-15.3),
                new MinimumDefaultPolicyVarianceAdjuster(48.2),
                new SegmentDefaultPolicyVarianceAdjuster(new Dictionary<string, double> { ["south"] = 0.83 }),
                new FlaggedDefaultPolicyVarianceAdjuster(flags, "default-policy-variance-rolling", -3.4),
            },
            new List<string>(),
            2.5,
            52375.5);
    }

    public double Price(DefaultPolicyVariance item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultPolicyVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
