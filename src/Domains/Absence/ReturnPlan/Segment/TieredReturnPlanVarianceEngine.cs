namespace Acme.Workforce.Absence.ReturnPlan.Segment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered return plan variance calculation.</summary>
public interface ITieredReturnPlanVarianceStep
{
    string Describe();
    double Adjust(TieredReturnPlanVariance item, double valueValue);
}

public sealed record ChannelTieredReturnPlanVarianceAdjuster(Dictionary<string, double> Factors) : ITieredReturnPlanVarianceStep
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(TieredReturnPlanVariance item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredTieredReturnPlanVarianceAdjuster(List<double> Bounds, List<double> Rates) : ITieredReturnPlanVarianceStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TieredReturnPlanVariance item, double valueValue)
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

public sealed record CapTieredReturnPlanVarianceAdjuster(double Cap) : ITieredReturnPlanVarianceStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TieredReturnPlanVariance item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumTieredReturnPlanVarianceAdjuster(double Minimum) : ITieredReturnPlanVarianceStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TieredReturnPlanVariance item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the tiered return plan variance steps in order and keeps a trace of each.</summary>
public sealed class TieredReturnPlanVarianceEngine(List<ITieredReturnPlanVarianceStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredReturnPlanVarianceStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredReturnPlanVarianceEngine DefaultTieredReturnPlanVarianceEngine()
    {
        return new TieredReturnPlanVarianceEngine(
            new List<ITieredReturnPlanVarianceStep>
            {
                new ChannelTieredReturnPlanVarianceAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.08, ["south"] = 0.93, ["central"] = 0.72 }),
                new TieredTieredReturnPlanVarianceAdjuster(
                    new List<double> { 2624.0, 5582.0, 8450.0 },
                    new List<double> { 1.13, 0.96, 0.92 }),
                new CapTieredReturnPlanVarianceAdjuster(22611.0),
                new MinimumTieredReturnPlanVarianceAdjuster(33.7),
            },
            new List<string>(),
            1.5,
            98749.2);
    }

    public double Run(TieredReturnPlanVariance item)
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

    public Dictionary<string, double> EvaluateAll(List<TieredReturnPlanVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
