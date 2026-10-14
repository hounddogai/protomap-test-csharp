namespace Acme.Workforce.Compliance.Exception.Plan;

/// <summary>One step of the pending exception plan calculation.</summary>
public interface IPendingExceptionPlanModifier
{
    string Describe();
    double Adjust(PendingExceptionPlan item, double valueValue);
}

public sealed record SegmentPendingExceptionPlanAdjuster(Dictionary<string, double> Factors) : IPendingExceptionPlanModifier
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(PendingExceptionPlan item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumPendingExceptionPlanAdjuster(double Minimum) : IPendingExceptionPlanModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PendingExceptionPlan item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentPendingExceptionPlanAdjuster(double Percent) : IPendingExceptionPlanModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PendingExceptionPlan item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the pending exception plan steps in order and keeps a trace of each.</summary>
public sealed class PendingExceptionPlanEngine(List<IPendingExceptionPlanModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingExceptionPlanModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingExceptionPlanEngine DefaultPendingExceptionPlanEngine()
    {
        return new PendingExceptionPlanEngine(
            new List<IPendingExceptionPlanModifier>
            {
                new SegmentPendingExceptionPlanAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.26, ["online"] = 0.74, ["retail"] = 1.38 }),
                new MinimumPendingExceptionPlanAdjuster(17.6),
                new PercentPendingExceptionPlanAdjuster(4.8),
            },
            new List<string>(),
            2.1,
            33231.8);
    }

    public double Compute(PendingExceptionPlan item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<PendingExceptionPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
