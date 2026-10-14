namespace Acme.Workforce.Compensation.Adjustment.Profile;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted adjustment summary calculation.</summary>
public interface IWeightedAdjustmentSummaryModifier
{
    string Describe();
    double Adjust(WeightedAdjustmentSummary item, double valueValue);
}

public sealed record RoundingWeightedAdjustmentSummaryAdjuster(double Step) : IWeightedAdjustmentSummaryModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WeightedAdjustmentSummary item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record WeightedAdjustmentSummaryCeilingOffsetAdjuster(double Weight) : IWeightedAdjustmentSummaryModifier
{
    public string Describe()
    {
        return $"plus ceiling times {Weight}";
    }

    public double Adjust(WeightedAdjustmentSummary item, double valueValue)
    {
        return valueValue + item.Ceiling * Weight;
    }
}

public sealed record PercentWeightedAdjustmentSummaryAdjuster(double Percent) : IWeightedAdjustmentSummaryModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WeightedAdjustmentSummary item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelWeightedAdjustmentSummaryAdjuster(Dictionary<string, double> Factors) : IWeightedAdjustmentSummaryModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(WeightedAdjustmentSummary item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedWeightedAdjustmentSummaryAdjuster(Registry Flags, string Flag, double Percent) : IWeightedAdjustmentSummaryModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WeightedAdjustmentSummary item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the weighted adjustment summary steps in order and keeps a trace of each.</summary>
public sealed class WeightedAdjustmentSummaryEngine(List<IWeightedAdjustmentSummaryModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedAdjustmentSummaryModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedAdjustmentSummaryEngine DefaultWeightedAdjustmentSummaryEngine(Registry flags)
    {
        return new WeightedAdjustmentSummaryEngine(
            new List<IWeightedAdjustmentSummaryModifier>
            {
                new RoundingWeightedAdjustmentSummaryAdjuster(5.0),
                new WeightedAdjustmentSummaryCeilingOffsetAdjuster(1.47),
                new PercentWeightedAdjustmentSummaryAdjuster(-0.5),
                new LabelWeightedAdjustmentSummaryAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.78, ["north"] = 0.89, ["east"] = 0.77 }),
                new FlaggedWeightedAdjustmentSummaryAdjuster(flags, "weighted-adjustment-summary-global", 1.7),
            },
            new List<string>(),
            5.0,
            85970.2);
    }

    public double Run(WeightedAdjustmentSummary item)
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

    public Dictionary<string, double> EvaluateAll(List<WeightedAdjustmentSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
