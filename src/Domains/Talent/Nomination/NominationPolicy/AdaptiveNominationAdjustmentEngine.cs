namespace Acme.Workforce.Talent.Nomination.NominationPolicy;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive nomination adjustment calculation.</summary>
public interface IAdaptiveNominationAdjustmentStep
{
    string Describe();
    double Adjust(AdaptiveNominationAdjustment item, double valueValue);
}

public sealed record AdaptiveNominationAdjustmentHoursOffsetAdjuster(double Weight) : IAdaptiveNominationAdjustmentStep
{
    public string Describe()
    {
        return $"plus hours times {Weight}";
    }

    public double Adjust(AdaptiveNominationAdjustment item, double valueValue)
    {
        return valueValue + item.Hours * Weight;
    }
}

public sealed record RoundingAdaptiveNominationAdjustmentAdjuster(double Step) : IAdaptiveNominationAdjustmentStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptiveNominationAdjustment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedAdaptiveNominationAdjustmentAdjuster(Registry Flags, string Flag, double Percent) : IAdaptiveNominationAdjustmentStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdaptiveNominationAdjustment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumAdaptiveNominationAdjustmentAdjuster(double Minimum) : IAdaptiveNominationAdjustmentStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptiveNominationAdjustment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapAdaptiveNominationAdjustmentAdjuster(double Cap) : IAdaptiveNominationAdjustmentStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptiveNominationAdjustment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentAdaptiveNominationAdjustmentAdjuster(double Percent) : IAdaptiveNominationAdjustmentStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdaptiveNominationAdjustment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the adaptive nomination adjustment steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveNominationAdjustmentEngine(List<IAdaptiveNominationAdjustmentStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveNominationAdjustmentStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveNominationAdjustmentEngine DefaultAdaptiveNominationAdjustmentEngine(Registry flags)
    {
        return new AdaptiveNominationAdjustmentEngine(
            new List<IAdaptiveNominationAdjustmentStep>
            {
                new AdaptiveNominationAdjustmentHoursOffsetAdjuster(0.97),
                new RoundingAdaptiveNominationAdjustmentAdjuster(0.25),
                new FlaggedAdaptiveNominationAdjustmentAdjuster(
                    flags,
                    "adaptive-nomination-adjustment-scheduled",
                    3.1),
                new MinimumAdaptiveNominationAdjustmentAdjuster(10.2),
                new CapAdaptiveNominationAdjustmentAdjuster(11418.0),
                new PercentAdaptiveNominationAdjustmentAdjuster(1.0),
            },
            new List<string>(),
            0.8,
            23723.8);
    }

    public double Compute(AdaptiveNominationAdjustment item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveNominationAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
