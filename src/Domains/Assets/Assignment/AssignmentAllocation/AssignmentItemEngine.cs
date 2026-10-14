namespace Acme.Workforce.Assets.Assignment.AssignmentAllocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the assignment item calculation.</summary>
public interface IAssignmentItemAdjuster
{
    string Describe();
    double Adjust(AssignmentItem item, double valueValue);
}

public sealed record AssignmentItemUpdatedAtOffsetAdjuster(double Weight) : IAssignmentItemAdjuster
{
    public string Describe()
    {
        return $"plus updated at times {Weight}";
    }

    public double Adjust(AssignmentItem item, double valueValue)
    {
        return valueValue + (double)item.UpdatedAt * Weight;
    }
}

public sealed record PercentAssignmentItemAdjuster(double Percent) : IAssignmentItemAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AssignmentItem item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingAssignmentItemAdjuster(double Step) : IAssignmentItemAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AssignmentItem item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the assignment item steps in order and keeps a trace of each.</summary>
public sealed class AssignmentItemEngine(List<IAssignmentItemAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAssignmentItemAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AssignmentItemEngine DefaultAssignmentItemEngine()
    {
        return new AssignmentItemEngine(
            new List<IAssignmentItemAdjuster>
            {
                new AssignmentItemUpdatedAtOffsetAdjuster(1.58),
                new PercentAssignmentItemAdjuster(-13.5),
                new RoundingAssignmentItemAdjuster(0.05),
            },
            new List<string>(),
            0.2,
            50440.9);
    }

    public double Evaluate(AssignmentItem item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<AssignmentItem> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
