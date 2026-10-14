namespace Acme.Workforce.Engagement.ActionPlan.Snapshot;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict action plan threshold calculation.</summary>
public interface IStrictActionPlanThresholdFactor
{
    string Describe();
    double Adjust(StrictActionPlanThreshold item, double valueValue);
}

public sealed record RoundingStrictActionPlanThresholdAdjuster(double Step) : IStrictActionPlanThresholdFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictActionPlanThreshold item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record StrictActionPlanThresholdRevisionOffsetAdjuster(double Weight) : IStrictActionPlanThresholdFactor
{
    public string Describe()
    {
        return $"plus revision times {Weight}";
    }

    public double Adjust(StrictActionPlanThreshold item, double valueValue)
    {
        return valueValue + (double)item.Revision * Weight;
    }
}

public sealed record PercentStrictActionPlanThresholdAdjuster(double Percent) : IStrictActionPlanThresholdFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StrictActionPlanThreshold item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumStrictActionPlanThresholdAdjuster(double Minimum) : IStrictActionPlanThresholdFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StrictActionPlanThreshold item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the strict action plan threshold steps in order and keeps a trace of each.</summary>
public sealed class StrictActionPlanThresholdEngine(List<IStrictActionPlanThresholdFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictActionPlanThresholdFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictActionPlanThresholdEngine DefaultStrictActionPlanThresholdEngine()
    {
        return new StrictActionPlanThresholdEngine(
            new List<IStrictActionPlanThresholdFactor>
            {
                new RoundingStrictActionPlanThresholdAdjuster(1.0),
                new StrictActionPlanThresholdRevisionOffsetAdjuster(1.11),
                new PercentStrictActionPlanThresholdAdjuster(18.6),
                new MinimumStrictActionPlanThresholdAdjuster(44.9),
            },
            new List<string>(),
            2.6,
            57745.8);
    }

    public double Compute(StrictActionPlanThreshold item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<StrictActionPlanThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
