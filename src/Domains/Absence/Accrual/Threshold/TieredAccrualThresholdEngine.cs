namespace Acme.Workforce.Absence.Accrual.Threshold;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered accrual threshold calculation.</summary>
public interface ITieredAccrualThresholdFactor
{
    string Describe();
    double Adjust(TieredAccrualThreshold item, double valueValue);
}

public sealed record TieredAccrualThresholdFteOffsetAdjuster(double Weight) : ITieredAccrualThresholdFactor
{
    public string Describe()
    {
        return $"plus fte times {Weight}";
    }

    public double Adjust(TieredAccrualThreshold item, double valueValue)
    {
        return valueValue + item.Fte * Weight;
    }
}

public sealed record RoundingTieredAccrualThresholdAdjuster(double Step) : ITieredAccrualThresholdFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TieredAccrualThreshold item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapTieredAccrualThresholdAdjuster(double Cap) : ITieredAccrualThresholdFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TieredAccrualThreshold item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedTieredAccrualThresholdAdjuster(Registry Flags, string Flag, double Percent) : ITieredAccrualThresholdFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TieredAccrualThreshold item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SegmentTieredAccrualThresholdAdjuster(Dictionary<string, double> Factors) : ITieredAccrualThresholdFactor
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(TieredAccrualThreshold item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the tiered accrual threshold steps in order and keeps a trace of each.</summary>
public sealed class TieredAccrualThresholdEngine(List<ITieredAccrualThresholdFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredAccrualThresholdFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredAccrualThresholdEngine DefaultTieredAccrualThresholdEngine(Registry flags)
    {
        return new TieredAccrualThresholdEngine(
            new List<ITieredAccrualThresholdFactor>
            {
                new TieredAccrualThresholdFteOffsetAdjuster(1.36),
                new RoundingTieredAccrualThresholdAdjuster(5.0),
                new CapTieredAccrualThresholdAdjuster(27311.0),
                new FlaggedTieredAccrualThresholdAdjuster(flags, "tiered-accrual-threshold-primary", -9.1),
                new SegmentTieredAccrualThresholdAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.87, ["online"] = 1.14, ["south"] = 1.06 }),
            },
            new List<string>(),
            4.8,
            76918.6);
    }

    public double Evaluate(TieredAccrualThreshold item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<TieredAccrualThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
