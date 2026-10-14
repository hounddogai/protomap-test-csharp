namespace Acme.Workforce.Workplace.Visitor.VisitorEntry;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary visitor assignment calculation.</summary>
public interface IPrimaryVisitorAssignmentFactor
{
    string Describe();
    double Adjust(PrimaryVisitorAssignment item, double valueValue);
}

public sealed record LabelPrimaryVisitorAssignmentAdjuster(Dictionary<string, double> Factors) : IPrimaryVisitorAssignmentFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(PrimaryVisitorAssignment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentPrimaryVisitorAssignmentAdjuster(double Percent) : IPrimaryVisitorAssignmentFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PrimaryVisitorAssignment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingPrimaryVisitorAssignmentAdjuster(double Step) : IPrimaryVisitorAssignmentFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PrimaryVisitorAssignment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the primary visitor assignment steps in order and keeps a trace of each.</summary>
public sealed class PrimaryVisitorAssignmentEngine(List<IPrimaryVisitorAssignmentFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryVisitorAssignmentFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryVisitorAssignmentEngine DefaultPrimaryVisitorAssignmentEngine()
    {
        return new PrimaryVisitorAssignmentEngine(
            new List<IPrimaryVisitorAssignmentFactor>
            {
                new LabelPrimaryVisitorAssignmentAdjuster(new Dictionary<string, double> { ["retail"] = 0.78 }),
                new PercentPrimaryVisitorAssignmentAdjuster(23.5),
                new RoundingPrimaryVisitorAssignmentAdjuster(0.05),
            },
            new List<string>(),
            1.5,
            15112.7);
    }

    public double Evaluate(PrimaryVisitorAssignment item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryVisitorAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
