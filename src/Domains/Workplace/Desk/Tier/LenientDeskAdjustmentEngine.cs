namespace Acme.Workforce.Workplace.Desk.Tier;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient desk adjustment calculation.</summary>
public interface ILenientDeskAdjustmentFactor
{
    string Describe();
    double Adjust(LenientDeskAdjustment item, double valueValue);
}

public sealed record MinimumLenientDeskAdjustmentAdjuster(double Minimum) : ILenientDeskAdjustmentFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientDeskAdjustment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapLenientDeskAdjustmentAdjuster(double Cap) : ILenientDeskAdjustmentFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LenientDeskAdjustment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentLenientDeskAdjustmentAdjuster(double Percent) : ILenientDeskAdjustmentFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientDeskAdjustment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelLenientDeskAdjustmentAdjuster(Dictionary<string, double> Factors) : ILenientDeskAdjustmentFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(LenientDeskAdjustment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedLenientDeskAdjustmentAdjuster(Registry Flags, string Flag, double Percent) : ILenientDeskAdjustmentFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientDeskAdjustment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredLenientDeskAdjustmentAdjuster(List<double> Bounds, List<double> Rates) : ILenientDeskAdjustmentFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientDeskAdjustment item, double valueValue)
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

/// <summary>Runs the lenient desk adjustment steps in order and keeps a trace of each.</summary>
public sealed class LenientDeskAdjustmentEngine(List<ILenientDeskAdjustmentFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientDeskAdjustmentFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientDeskAdjustmentEngine DefaultLenientDeskAdjustmentEngine(Registry flags)
    {
        return new LenientDeskAdjustmentEngine(
            new List<ILenientDeskAdjustmentFactor>
            {
                new MinimumLenientDeskAdjustmentAdjuster(24.9),
                new CapLenientDeskAdjustmentAdjuster(10911.0),
                new PercentLenientDeskAdjustmentAdjuster(7.2),
                new LabelLenientDeskAdjustmentAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.75, ["retail"] = 1.07, ["west"] = 1.28 }),
                new FlaggedLenientDeskAdjustmentAdjuster(flags, "lenient-desk-adjustment-strict", 1.8),
                new TieredLenientDeskAdjustmentAdjuster(
                    new List<double> { 2608.0, 5851.0, 7219.0, 8278.0 },
                    new List<double> { 0.85, 1.03, 1.19, 1.03 }),
            },
            new List<string>(),
            4.0,
            43208.0);
    }

    public double Price(LenientDeskAdjustment item)
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

    public Dictionary<string, double> EvaluateAll(List<LenientDeskAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
