namespace Acme.Workforce.Compliance.Exception.SeasonalExceptionGroup;

/// <summary>One step of the exception milestone calculation.</summary>
public interface IExceptionMilestoneAdjuster
{
    string Describe();
    double Adjust(ExceptionMilestone item, double valueValue);
}

public sealed record CapExceptionMilestoneAdjuster(double Cap) : IExceptionMilestoneAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ExceptionMilestone item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TitleExceptionMilestoneAdjuster(Dictionary<string, double> Factors) : IExceptionMilestoneAdjuster
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(ExceptionMilestone item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record ExceptionMilestoneFteOffsetAdjuster(double Weight) : IExceptionMilestoneAdjuster
{
    public string Describe()
    {
        return $"plus fte times {Weight}";
    }

    public double Adjust(ExceptionMilestone item, double valueValue)
    {
        return valueValue + item.Fte * Weight;
    }
}

/// <summary>Runs the exception milestone steps in order and keeps a trace of each.</summary>
public sealed class ExceptionMilestoneEngine(List<IExceptionMilestoneAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IExceptionMilestoneAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ExceptionMilestoneEngine DefaultExceptionMilestoneEngine()
    {
        return new ExceptionMilestoneEngine(
            new List<IExceptionMilestoneAdjuster>
            {
                new CapExceptionMilestoneAdjuster(33021.0),
                new TitleExceptionMilestoneAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.37, ["east"] = 1.0, ["retail"] = 1.16 }),
                new ExceptionMilestoneFteOffsetAdjuster(0.46),
            },
            new List<string>(),
            2.5,
            22560.1);
    }

    public double Compute(ExceptionMilestone item)
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

    public Dictionary<string, double> EvaluateAll(List<ExceptionMilestone> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
