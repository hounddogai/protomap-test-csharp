namespace Acme.Workforce.Compliance.TrainingDue.CappedDueVariance;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the adaptive training due claim calculation.</summary>
public interface IAdaptiveTrainingDueClaimModifier
{
    string Describe();
    double Adjust(AdaptiveTrainingDueClaim item, double valueValue);
}

public sealed record MinimumAdaptiveTrainingDueClaimAdjuster(double Minimum) : IAdaptiveTrainingDueClaimModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptiveTrainingDueClaim item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentAdaptiveTrainingDueClaimAdjuster(double Percent) : IAdaptiveTrainingDueClaimModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdaptiveTrainingDueClaim item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TitleAdaptiveTrainingDueClaimAdjuster(Dictionary<string, double> Factors) : IAdaptiveTrainingDueClaimModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(AdaptiveTrainingDueClaim item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapAdaptiveTrainingDueClaimAdjuster(double Cap) : IAdaptiveTrainingDueClaimModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptiveTrainingDueClaim item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record AdaptiveTrainingDueClaimWeightOffsetAdjuster(double Weight) : IAdaptiveTrainingDueClaimModifier
{
    public string Describe()
    {
        return $"plus weight times {Weight}";
    }

    public double Adjust(AdaptiveTrainingDueClaim item, double valueValue)
    {
        return valueValue + item.Weight * Weight;
    }
}

public sealed record FlaggedAdaptiveTrainingDueClaimAdjuster(Registry Flags, string Flag, double Percent) : IAdaptiveTrainingDueClaimModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdaptiveTrainingDueClaim item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the adaptive training due claim steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveTrainingDueClaimEngine(List<IAdaptiveTrainingDueClaimModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveTrainingDueClaimModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveTrainingDueClaimEngine DefaultAdaptiveTrainingDueClaimEngine(Registry flags)
    {
        return new AdaptiveTrainingDueClaimEngine(
            new List<IAdaptiveTrainingDueClaimModifier>
            {
                new MinimumAdaptiveTrainingDueClaimAdjuster(3.4),
                new PercentAdaptiveTrainingDueClaimAdjuster(18.7),
                new TitleAdaptiveTrainingDueClaimAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.25, ["central"] = 1.37 }),
                new CapAdaptiveTrainingDueClaimAdjuster(12980.0),
                new AdaptiveTrainingDueClaimWeightOffsetAdjuster(0.72),
                new FlaggedAdaptiveTrainingDueClaimAdjuster(flags, "adaptive-training-due-claim-primary", -2.7),
            },
            new List<string>(),
            2.2,
            34736.8);
    }

    public double Evaluate(AdaptiveTrainingDueClaim item)
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveTrainingDueClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
