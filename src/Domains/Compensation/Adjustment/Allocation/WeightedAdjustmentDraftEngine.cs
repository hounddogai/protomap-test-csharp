namespace Acme.Workforce.Compensation.Adjustment.Allocation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted adjustment draft calculation.</summary>
public interface IWeightedAdjustmentDraftStep
{
    string Describe();
    double Adjust(WeightedAdjustmentDraft item, double valueValue);
}

public sealed record RoundingWeightedAdjustmentDraftAdjuster(double Step) : IWeightedAdjustmentDraftStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WeightedAdjustmentDraft item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LabelWeightedAdjustmentDraftAdjuster(Dictionary<string, double> Factors) : IWeightedAdjustmentDraftStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(WeightedAdjustmentDraft item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedWeightedAdjustmentDraftAdjuster(Registry Flags, string Flag, double Percent) : IWeightedAdjustmentDraftStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WeightedAdjustmentDraft item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the weighted adjustment draft steps in order and keeps a trace of each.</summary>
public sealed class WeightedAdjustmentDraftEngine(List<IWeightedAdjustmentDraftStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedAdjustmentDraftStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedAdjustmentDraftEngine DefaultWeightedAdjustmentDraftEngine(Registry flags)
    {
        return new WeightedAdjustmentDraftEngine(
            new List<IWeightedAdjustmentDraftStep>
            {
                new RoundingWeightedAdjustmentDraftAdjuster(0.05),
                new LabelWeightedAdjustmentDraftAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.32, ["north"] = 1.1 }),
                new FlaggedWeightedAdjustmentDraftAdjuster(flags, "weighted-adjustment-draft-default", 0.5),
            },
            new List<string>(),
            3.5,
            55781.3);
    }

    public double Compute(WeightedAdjustmentDraft item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedAdjustmentDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
