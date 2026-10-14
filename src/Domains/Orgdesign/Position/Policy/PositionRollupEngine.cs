namespace Acme.Workforce.Orgdesign.Position.Policy;

/// <summary>One step of the position rollup calculation.</summary>
public interface IPositionRollupFactor
{
    string Describe();
    double Adjust(PositionRollup item, double valueValue);
}

public sealed record TieredPositionRollupAdjuster(List<double> Bounds, List<double> Rates) : IPositionRollupFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PositionRollup item, double valueValue)
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

public sealed record MinimumPositionRollupAdjuster(double Minimum) : IPositionRollupFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PositionRollup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PositionRollupQuantityOffsetAdjuster(double Weight) : IPositionRollupFactor
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(PositionRollup item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

/// <summary>Runs the position rollup steps in order and keeps a trace of each.</summary>
public sealed class PositionRollupEngine(List<IPositionRollupFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPositionRollupFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PositionRollupEngine DefaultPositionRollupEngine()
    {
        return new PositionRollupEngine(
            new List<IPositionRollupFactor>
            {
                new TieredPositionRollupAdjuster(
                    new List<double> { 1482.0, 7451.0 },
                    new List<double> { 1.15, 1.05 }),
                new MinimumPositionRollupAdjuster(38.2),
                new PositionRollupQuantityOffsetAdjuster(1.02),
            },
            new List<string>(),
            4.7,
            93504.4);
    }

    public double Evaluate(PositionRollup item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<PositionRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
