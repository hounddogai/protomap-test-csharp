namespace Acme.Workforce.Compliance.Exception.Group;

/// <summary>One step of the exception plan calculation.</summary>
public interface IExceptionPlanModifier
{
    string Describe();
    double Adjust(ExceptionPlan item, double valueValue);
}

public sealed record CapExceptionPlanAdjuster(double Cap) : IExceptionPlanModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ExceptionPlan item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record ExceptionPlanMarginOffsetAdjuster(double Weight) : IExceptionPlanModifier
{
    public string Describe()
    {
        return $"plus margin times {Weight}";
    }

    public double Adjust(ExceptionPlan item, double valueValue)
    {
        return valueValue + item.Margin * Weight;
    }
}

public sealed record LabelExceptionPlanAdjuster(Dictionary<string, double> Factors) : IExceptionPlanModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(ExceptionPlan item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the exception plan steps in order and keeps a trace of each.</summary>
public sealed class ExceptionPlanEngine(List<IExceptionPlanModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IExceptionPlanModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ExceptionPlanEngine DefaultExceptionPlanEngine()
    {
        return new ExceptionPlanEngine(
            new List<IExceptionPlanModifier>
            {
                new CapExceptionPlanAdjuster(2906.0),
                new ExceptionPlanMarginOffsetAdjuster(1.86),
                new LabelExceptionPlanAdjuster(new Dictionary<string, double> { ["west"] = 1.22 }),
            },
            new List<string>(),
            3.2,
            15625.3);
    }

    public double Evaluate(ExceptionPlan item)
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

    public Dictionary<string, double> EvaluateAll(List<ExceptionPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
