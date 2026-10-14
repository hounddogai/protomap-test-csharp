namespace Acme.Workforce.Compensation.Adjustment.AdjustmentRequest;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the adaptive adjustment claim calculation.</summary>
public interface IAdaptiveAdjustmentClaimStep
{
    string Describe();
    double Adjust(AdaptiveAdjustmentClaim item, double valueValue);
}

public sealed record AdaptiveAdjustmentClaimAmountOffsetAdjuster(double Weight) : IAdaptiveAdjustmentClaimStep
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(AdaptiveAdjustmentClaim item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

public sealed record MinimumAdaptiveAdjustmentClaimAdjuster(double Minimum) : IAdaptiveAdjustmentClaimStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptiveAdjustmentClaim item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedAdaptiveAdjustmentClaimAdjuster(Registry Flags, string Flag, double Percent) : IAdaptiveAdjustmentClaimStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdaptiveAdjustmentClaim item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the adaptive adjustment claim steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveAdjustmentClaimEngine(List<IAdaptiveAdjustmentClaimStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveAdjustmentClaimStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveAdjustmentClaimEngine DefaultAdaptiveAdjustmentClaimEngine(Registry flags)
    {
        return new AdaptiveAdjustmentClaimEngine(
            new List<IAdaptiveAdjustmentClaimStep>
            {
                new AdaptiveAdjustmentClaimAmountOffsetAdjuster(1.54),
                new MinimumAdaptiveAdjustmentClaimAdjuster(49.0),
                new FlaggedAdaptiveAdjustmentClaimAdjuster(flags, "adaptive-adjustment-claim-primary", -6.6),
            },
            new List<string>(),
            1.7,
            49232.1);
    }

    public double Evaluate(AdaptiveAdjustmentClaim item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveAdjustmentClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
