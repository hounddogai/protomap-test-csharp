namespace Acme.Workforce.Absence.Policy.Threshold;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient policy threshold calculation.</summary>
public interface ILenientPolicyThresholdAdjuster
{
    string Describe();
    double Adjust(LenientPolicyThreshold item, double valueValue);
}

public sealed record FlaggedLenientPolicyThresholdAdjuster(Registry Flags, string Flag, double Percent) : ILenientPolicyThresholdAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientPolicyThreshold item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumLenientPolicyThresholdAdjuster(double Minimum) : ILenientPolicyThresholdAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientPolicyThreshold item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentLenientPolicyThresholdAdjuster(double Percent) : ILenientPolicyThresholdAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientPolicyThreshold item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LenientPolicyThresholdDurationMinutesOffsetAdjuster(double Weight) : ILenientPolicyThresholdAdjuster
{
    public string Describe()
    {
        return $"plus duration minutes times {Weight}";
    }

    public double Adjust(LenientPolicyThreshold item, double valueValue)
    {
        return valueValue + (double)item.DurationMinutes * Weight;
    }
}

public sealed record NameLenientPolicyThresholdAdjuster(Dictionary<string, double> Factors) : ILenientPolicyThresholdAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(LenientPolicyThreshold item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapLenientPolicyThresholdAdjuster(double Cap) : ILenientPolicyThresholdAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LenientPolicyThreshold item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the lenient policy threshold steps in order and keeps a trace of each.</summary>
public sealed class LenientPolicyThresholdEngine(List<ILenientPolicyThresholdAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientPolicyThresholdAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientPolicyThresholdEngine DefaultLenientPolicyThresholdEngine(Registry flags)
    {
        return new LenientPolicyThresholdEngine(
            new List<ILenientPolicyThresholdAdjuster>
            {
                new FlaggedLenientPolicyThresholdAdjuster(flags, "lenient-policy-threshold-rolling", 2.2),
                new MinimumLenientPolicyThresholdAdjuster(37.7),
                new PercentLenientPolicyThresholdAdjuster(-18.8),
                new LenientPolicyThresholdDurationMinutesOffsetAdjuster(0.13),
                new NameLenientPolicyThresholdAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.92, ["west"] = 1.1, ["north"] = 1.03 }),
                new CapLenientPolicyThresholdAdjuster(721.0),
            },
            new List<string>(),
            3.4,
            66777.1);
    }

    public double Run(LenientPolicyThreshold item)
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

    public Dictionary<string, double> EvaluateAll(List<LenientPolicyThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
