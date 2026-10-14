namespace Acme.Workforce.Engagement.ActionPlan.Metric;

/// <summary>One step of the default action plan draft calculation.</summary>
public interface IDefaultActionPlanDraftAdjuster
{
    string Describe();
    double Adjust(DefaultActionPlanDraft item, double valueValue);
}

public sealed record MinimumDefaultActionPlanDraftAdjuster(double Minimum) : IDefaultActionPlanDraftAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultActionPlanDraft item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record DefaultActionPlanDraftBudgetOffsetAdjuster(double Weight) : IDefaultActionPlanDraftAdjuster
{
    public string Describe()
    {
        return $"plus budget times {Weight}";
    }

    public double Adjust(DefaultActionPlanDraft item, double valueValue)
    {
        return valueValue + item.Budget * Weight;
    }
}

public sealed record CapDefaultActionPlanDraftAdjuster(double Cap) : IDefaultActionPlanDraftAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultActionPlanDraft item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the default action plan draft steps in order and keeps a trace of each.</summary>
public sealed class DefaultActionPlanDraftEngine(List<IDefaultActionPlanDraftAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultActionPlanDraftAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultActionPlanDraftEngine DefaultDefaultActionPlanDraftEngine()
    {
        return new DefaultActionPlanDraftEngine(
            new List<IDefaultActionPlanDraftAdjuster>
            {
                new MinimumDefaultActionPlanDraftAdjuster(26.3),
                new DefaultActionPlanDraftBudgetOffsetAdjuster(1.2),
                new CapDefaultActionPlanDraftAdjuster(11510.0),
            },
            new List<string>(),
            2.5,
            4095.5);
    }

    public double Run(DefaultActionPlanDraft item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultActionPlanDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
