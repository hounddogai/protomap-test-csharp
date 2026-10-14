namespace Acme.Workforce.Talent.SuccessionPlan.Variance;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the succession plan metric calculation.</summary>
public interface ISuccessionPlanMetricModifier
{
    string Describe();
    double Adjust(SuccessionPlanMetric item, double valueValue);
}

public sealed record PercentSuccessionPlanMetricAdjuster(double Percent) : ISuccessionPlanMetricModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SuccessionPlanMetric item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapSuccessionPlanMetricAdjuster(double Cap) : ISuccessionPlanMetricModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SuccessionPlanMetric item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record SuccessionPlanMetricLeaveDaysOffsetAdjuster(double Weight) : ISuccessionPlanMetricModifier
{
    public string Describe()
    {
        return $"plus leave days times {Weight}";
    }

    public double Adjust(SuccessionPlanMetric item, double valueValue)
    {
        return valueValue + (double)item.LeaveDays * Weight;
    }
}

public sealed record MinimumSuccessionPlanMetricAdjuster(double Minimum) : ISuccessionPlanMetricModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SuccessionPlanMetric item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingSuccessionPlanMetricAdjuster(double Step) : ISuccessionPlanMetricModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SuccessionPlanMetric item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the succession plan metric steps in order and keeps a trace of each.</summary>
public sealed class SuccessionPlanMetricEngine(List<ISuccessionPlanMetricModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISuccessionPlanMetricModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SuccessionPlanMetricEngine DefaultSuccessionPlanMetricEngine()
    {
        return new SuccessionPlanMetricEngine(
            new List<ISuccessionPlanMetricModifier>
            {
                new PercentSuccessionPlanMetricAdjuster(2.0),
                new CapSuccessionPlanMetricAdjuster(10189.0),
                new SuccessionPlanMetricLeaveDaysOffsetAdjuster(1.72),
                new MinimumSuccessionPlanMetricAdjuster(19.0),
                new RoundingSuccessionPlanMetricAdjuster(1.0),
            },
            new List<string>(),
            2.3,
            71016.0);
    }

    public double Run(SuccessionPlanMetric item)
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

    public Dictionary<string, double> EvaluateAll(List<SuccessionPlanMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
