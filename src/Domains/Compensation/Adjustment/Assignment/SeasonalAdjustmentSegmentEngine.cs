namespace Acme.Workforce.Compensation.Adjustment.Assignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal adjustment segment calculation.</summary>
public interface ISeasonalAdjustmentSegmentStep
{
    string Describe();
    double Adjust(SeasonalAdjustmentSegment item, double valueValue);
}

public sealed record RoundingSeasonalAdjustmentSegmentAdjuster(double Step) : ISeasonalAdjustmentSegmentStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SeasonalAdjustmentSegment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredSeasonalAdjustmentSegmentAdjuster(List<double> Bounds, List<double> Rates) : ISeasonalAdjustmentSegmentStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SeasonalAdjustmentSegment item, double valueValue)
    {
        for (var index = 0; index < Bounds.Count; index++)
        {
            if (valueValue <= Bounds[index] && index < Rates.Count)
            {
                return valueValue * Rates[index];
            }
        }
        return valueValue;
    }
}

public sealed record MinimumSeasonalAdjustmentSegmentAdjuster(double Minimum) : ISeasonalAdjustmentSegmentStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SeasonalAdjustmentSegment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the seasonal adjustment segment steps in order and keeps a trace of each.</summary>
public sealed class SeasonalAdjustmentSegmentEngine(List<ISeasonalAdjustmentSegmentStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalAdjustmentSegmentStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalAdjustmentSegmentEngine DefaultSeasonalAdjustmentSegmentEngine()
    {
        return new SeasonalAdjustmentSegmentEngine(
            new List<ISeasonalAdjustmentSegmentStep>
            {
                new RoundingSeasonalAdjustmentSegmentAdjuster(5.0),
                new TieredSeasonalAdjustmentSegmentAdjuster(
                    new List<double> { 1632.0, 7515.0, 7838.0, 8743.0 },
                    new List<double> { 1.06, 1.06, 1.03, 1.19 }),
                new MinimumSeasonalAdjustmentSegmentAdjuster(7.5),
            },
            new List<string>(),
            3.6,
            15717.2);
    }

    public double Compute(SeasonalAdjustmentSegment item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalAdjustmentSegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
