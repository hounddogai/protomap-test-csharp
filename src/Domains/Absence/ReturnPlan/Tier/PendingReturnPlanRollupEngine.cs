namespace Acme.Workforce.Absence.ReturnPlan.Tier;

/// <summary>One step of the pending return plan rollup calculation.</summary>
public interface IPendingReturnPlanRollupAdjuster
{
    string Describe();
    double Adjust(PendingReturnPlanRollup item, double valueValue);
}

public sealed record CapPendingReturnPlanRollupAdjuster(double Cap) : IPendingReturnPlanRollupAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PendingReturnPlanRollup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredPendingReturnPlanRollupAdjuster(List<double> Bounds, List<double> Rates) : IPendingReturnPlanRollupAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PendingReturnPlanRollup item, double valueValue)
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

public sealed record MinimumPendingReturnPlanRollupAdjuster(double Minimum) : IPendingReturnPlanRollupAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PendingReturnPlanRollup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PendingReturnPlanRollupCostOffsetAdjuster(double Weight) : IPendingReturnPlanRollupAdjuster
{
    public string Describe()
    {
        return $"plus cost times {Weight}";
    }

    public double Adjust(PendingReturnPlanRollup item, double valueValue)
    {
        return valueValue + item.Cost * Weight;
    }
}

/// <summary>Runs the pending return plan rollup steps in order and keeps a trace of each.</summary>
public sealed class PendingReturnPlanRollupEngine(List<IPendingReturnPlanRollupAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingReturnPlanRollupAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingReturnPlanRollupEngine DefaultPendingReturnPlanRollupEngine()
    {
        return new PendingReturnPlanRollupEngine(
            new List<IPendingReturnPlanRollupAdjuster>
            {
                new CapPendingReturnPlanRollupAdjuster(18051.0),
                new TieredPendingReturnPlanRollupAdjuster(
                    new List<double> { 3257.0, 7193.0, 9992.0 },
                    new List<double> { 1.01, 1.19, 0.84 }),
                new MinimumPendingReturnPlanRollupAdjuster(46.7),
                new PendingReturnPlanRollupCostOffsetAdjuster(1.18),
            },
            new List<string>(),
            0.4,
            75629.3);
    }

    public double Run(PendingReturnPlanRollup item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<PendingReturnPlanRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
