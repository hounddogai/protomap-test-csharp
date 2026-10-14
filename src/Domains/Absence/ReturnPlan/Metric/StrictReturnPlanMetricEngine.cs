namespace Acme.Workforce.Absence.ReturnPlan.Metric;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict return plan metric calculation.</summary>
public interface IStrictReturnPlanMetricModifier
{
    string Describe();
    double Adjust(StrictReturnPlanMetric item, double valueValue);
}

public sealed record CategoryStrictReturnPlanMetricAdjuster(Dictionary<string, double> Factors) : IStrictReturnPlanMetricModifier
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(StrictReturnPlanMetric item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumStrictReturnPlanMetricAdjuster(double Minimum) : IStrictReturnPlanMetricModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StrictReturnPlanMetric item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedStrictReturnPlanMetricAdjuster(Registry Flags, string Flag, double Percent) : IStrictReturnPlanMetricModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StrictReturnPlanMetric item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingStrictReturnPlanMetricAdjuster(double Step) : IStrictReturnPlanMetricModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictReturnPlanMetric item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the strict return plan metric steps in order and keeps a trace of each.</summary>
public sealed class StrictReturnPlanMetricEngine(List<IStrictReturnPlanMetricModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictReturnPlanMetricModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictReturnPlanMetricEngine DefaultStrictReturnPlanMetricEngine(Registry flags)
    {
        return new StrictReturnPlanMetricEngine(
            new List<IStrictReturnPlanMetricModifier>
            {
                new CategoryStrictReturnPlanMetricAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.35, ["east"] = 1.17 }),
                new MinimumStrictReturnPlanMetricAdjuster(4.6),
                new FlaggedStrictReturnPlanMetricAdjuster(flags, "strict-return-plan-metric-pending", 5.1),
                new RoundingStrictReturnPlanMetricAdjuster(0.1),
            },
            new List<string>(),
            5.0,
            54965.2);
    }

    public double Compute(StrictReturnPlanMetric item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<StrictReturnPlanMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
