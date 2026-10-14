namespace Acme.Workforce.Orgdesign.Reorg.ReorgProfile;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the reorg adjustment calculation.</summary>
public interface IReorgAdjustmentModifier
{
    string Describe();
    double Adjust(ReorgAdjustment item, double valueValue);
}

public sealed record MinimumReorgAdjustmentAdjuster(double Minimum) : IReorgAdjustmentModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ReorgAdjustment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingReorgAdjustmentAdjuster(double Step) : IReorgAdjustmentModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ReorgAdjustment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredReorgAdjustmentAdjuster(List<double> Bounds, List<double> Rates) : IReorgAdjustmentModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ReorgAdjustment item, double valueValue)
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

/// <summary>Runs the reorg adjustment steps in order and keeps a trace of each.</summary>
public sealed class ReorgAdjustmentEngine(List<IReorgAdjustmentModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IReorgAdjustmentModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ReorgAdjustmentEngine DefaultReorgAdjustmentEngine()
    {
        return new ReorgAdjustmentEngine(
            new List<IReorgAdjustmentModifier>
            {
                new MinimumReorgAdjustmentAdjuster(37.1),
                new RoundingReorgAdjustmentAdjuster(5.0),
                new TieredReorgAdjustmentAdjuster(
                    new List<double> { 2969.0, 8571.0 },
                    new List<double> { 0.96, 0.89 }),
            },
            new List<string>(),
            1.9,
            20368.9);
    }

    public double Run(ReorgAdjustment item)
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

    public Dictionary<string, double> EvaluateAll(List<ReorgAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
