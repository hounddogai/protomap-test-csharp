namespace Acme.Workforce.Compliance.Certification.Adjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive certification adjustment calculation.</summary>
public interface IAdaptiveCertificationAdjustmentModifier
{
    string Describe();
    double Adjust(AdaptiveCertificationAdjustment item, double valueValue);
}

public sealed record CapAdaptiveCertificationAdjustmentAdjuster(double Cap) : IAdaptiveCertificationAdjustmentModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptiveCertificationAdjustment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedAdaptiveCertificationAdjustmentAdjuster(Registry Flags, string Flag, double Percent) : IAdaptiveCertificationAdjustmentModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdaptiveCertificationAdjustment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record UnitAdaptiveCertificationAdjustmentAdjuster(Dictionary<string, double> Factors) : IAdaptiveCertificationAdjustmentModifier
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(AdaptiveCertificationAdjustment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumAdaptiveCertificationAdjustmentAdjuster(double Minimum) : IAdaptiveCertificationAdjustmentModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptiveCertificationAdjustment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the adaptive certification adjustment steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveCertificationAdjustmentEngine(List<IAdaptiveCertificationAdjustmentModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveCertificationAdjustmentModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveCertificationAdjustmentEngine DefaultAdaptiveCertificationAdjustmentEngine(Registry flags)
    {
        return new AdaptiveCertificationAdjustmentEngine(
            new List<IAdaptiveCertificationAdjustmentModifier>
            {
                new CapAdaptiveCertificationAdjustmentAdjuster(35894.0),
                new FlaggedAdaptiveCertificationAdjustmentAdjuster(
                    flags,
                    "adaptive-certification-adjustment-capped",
                    -8.0),
                new UnitAdaptiveCertificationAdjustmentAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.34, ["west"] = 1.28, ["north"] = 1.25 }),
                new MinimumAdaptiveCertificationAdjustmentAdjuster(4.6),
            },
            new List<string>(),
            3.8,
            89767.4);
    }

    public double Compute(AdaptiveCertificationAdjustment item)
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveCertificationAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
