namespace Acme.Workforce.Compensation.Adjustment.Rule;

/// <summary>One step of the adjustment rollup calculation.</summary>
public interface IAdjustmentRollupFactor
{
    string Describe();
    double Adjust(AdjustmentRollup item, double valueValue);
}

public sealed record CapAdjustmentRollupAdjuster(double Cap) : IAdjustmentRollupFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdjustmentRollup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumAdjustmentRollupAdjuster(double Minimum) : IAdjustmentRollupFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdjustmentRollup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record AdjustmentRollupLeaveDaysOffsetAdjuster(double Weight) : IAdjustmentRollupFactor
{
    public string Describe()
    {
        return $"plus leave days times {Weight}";
    }

    public double Adjust(AdjustmentRollup item, double valueValue)
    {
        return valueValue + (double)item.LeaveDays * Weight;
    }
}

/// <summary>Runs the adjustment rollup steps in order and keeps a trace of each.</summary>
public sealed class AdjustmentRollupEngine(List<IAdjustmentRollupFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdjustmentRollupFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdjustmentRollupEngine DefaultAdjustmentRollupEngine()
    {
        return new AdjustmentRollupEngine(
            new List<IAdjustmentRollupFactor>
            {
                new CapAdjustmentRollupAdjuster(41784.0),
                new MinimumAdjustmentRollupAdjuster(46.6),
                new AdjustmentRollupLeaveDaysOffsetAdjuster(0.59),
            },
            new List<string>(),
            2.6,
            3743.6);
    }

    public double Evaluate(AdjustmentRollup item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<AdjustmentRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
