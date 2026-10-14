namespace Acme.Workforce.Absence.Balance.Segment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual balance segment calculation.</summary>
public interface IManualBalanceSegmentModifier
{
    string Describe();
    double Adjust(ManualBalanceSegment item, double valueValue);
}

public sealed record CapManualBalanceSegmentAdjuster(double Cap) : IManualBalanceSegmentModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ManualBalanceSegment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredManualBalanceSegmentAdjuster(List<double> Bounds, List<double> Rates) : IManualBalanceSegmentModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ManualBalanceSegment item, double valueValue)
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

public sealed record RoundingManualBalanceSegmentAdjuster(double Step) : IManualBalanceSegmentModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ManualBalanceSegment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the manual balance segment steps in order and keeps a trace of each.</summary>
public sealed class ManualBalanceSegmentEngine(List<IManualBalanceSegmentModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualBalanceSegmentModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualBalanceSegmentEngine DefaultManualBalanceSegmentEngine()
    {
        return new ManualBalanceSegmentEngine(
            new List<IManualBalanceSegmentModifier>
            {
                new CapManualBalanceSegmentAdjuster(20897.0),
                new TieredManualBalanceSegmentAdjuster(
                    new List<double> { 2595.0, 6779.0, 9398.0 },
                    new List<double> { 0.91, 1.2, 0.83 }),
                new RoundingManualBalanceSegmentAdjuster(5.0),
            },
            new List<string>(),
            2.3,
            49589.9);
    }

    public double Evaluate(ManualBalanceSegment item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<ManualBalanceSegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
