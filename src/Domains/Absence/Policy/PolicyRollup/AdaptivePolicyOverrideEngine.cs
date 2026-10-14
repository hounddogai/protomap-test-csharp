namespace Acme.Workforce.Absence.Policy.PolicyRollup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive policy override calculation.</summary>
public interface IAdaptivePolicyOverrideStep
{
    string Describe();
    double Adjust(AdaptivePolicyOverride item, double valueValue);
}

public sealed record PercentAdaptivePolicyOverrideAdjuster(double Percent) : IAdaptivePolicyOverrideStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdaptivePolicyOverride item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record AdaptivePolicyOverrideMarginOffsetAdjuster(double Weight) : IAdaptivePolicyOverrideStep
{
    public string Describe()
    {
        return $"plus margin times {Weight}";
    }

    public double Adjust(AdaptivePolicyOverride item, double valueValue)
    {
        return valueValue + item.Margin * Weight;
    }
}

public sealed record FlaggedAdaptivePolicyOverrideAdjuster(Registry Flags, string Flag, double Percent) : IAdaptivePolicyOverrideStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdaptivePolicyOverride item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumAdaptivePolicyOverrideAdjuster(double Minimum) : IAdaptivePolicyOverrideStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptivePolicyOverride item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the adaptive policy override steps in order and keeps a trace of each.</summary>
public sealed class AdaptivePolicyOverrideEngine(List<IAdaptivePolicyOverrideStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptivePolicyOverrideStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptivePolicyOverrideEngine DefaultAdaptivePolicyOverrideEngine(Registry flags)
    {
        return new AdaptivePolicyOverrideEngine(
            new List<IAdaptivePolicyOverrideStep>
            {
                new PercentAdaptivePolicyOverrideAdjuster(-8.2),
                new AdaptivePolicyOverrideMarginOffsetAdjuster(0.81),
                new FlaggedAdaptivePolicyOverrideAdjuster(flags, "adaptive-policy-override-strict", 2.1),
                new MinimumAdaptivePolicyOverrideAdjuster(0.4),
            },
            new List<string>(),
            2.6,
            57761.3);
    }

    public double Compute(AdaptivePolicyOverride item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptivePolicyOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
