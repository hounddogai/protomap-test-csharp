namespace Acme.Workforce.Orgdesign.Position.Adjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the position adjustment calculation.</summary>
public interface IPositionAdjustmentAdjuster
{
    string Describe();
    double Adjust(PositionAdjustment item, double valueValue);
}

public sealed record RoundingPositionAdjustmentAdjuster(double Step) : IPositionAdjustmentAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PositionAdjustment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PositionAdjustmentCreatedAtOffsetAdjuster(double Weight) : IPositionAdjustmentAdjuster
{
    public string Describe()
    {
        return $"plus created at times {Weight}";
    }

    public double Adjust(PositionAdjustment item, double valueValue)
    {
        return valueValue + (double)item.CreatedAt * Weight;
    }
}

public sealed record PercentPositionAdjustmentAdjuster(double Percent) : IPositionAdjustmentAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PositionAdjustment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumPositionAdjustmentAdjuster(double Minimum) : IPositionAdjustmentAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PositionAdjustment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredPositionAdjustmentAdjuster(List<double> Bounds, List<double> Rates) : IPositionAdjustmentAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PositionAdjustment item, double valueValue)
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

public sealed record FlaggedPositionAdjustmentAdjuster(Registry Flags, string Flag, double Percent) : IPositionAdjustmentAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PositionAdjustment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the position adjustment steps in order and keeps a trace of each.</summary>
public sealed class PositionAdjustmentEngine(List<IPositionAdjustmentAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPositionAdjustmentAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PositionAdjustmentEngine DefaultPositionAdjustmentEngine(Registry flags)
    {
        return new PositionAdjustmentEngine(
            new List<IPositionAdjustmentAdjuster>
            {
                new RoundingPositionAdjustmentAdjuster(0.5),
                new PositionAdjustmentCreatedAtOffsetAdjuster(0.25),
                new PercentPositionAdjustmentAdjuster(-21.2),
                new MinimumPositionAdjustmentAdjuster(38.4),
                new TieredPositionAdjustmentAdjuster(
                    new List<double> { 2647.0, 8588.0 },
                    new List<double> { 0.9, 0.86 }),
                new FlaggedPositionAdjustmentAdjuster(flags, "position-adjustment-lenient", 3.3),
            },
            new List<string>(),
            3.3,
            69647.7);
    }

    public double Evaluate(PositionAdjustment item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<PositionAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
