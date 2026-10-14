namespace Acme.Workforce.Workplace.Visitor.Assignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the visitor assignment calculation.</summary>
public interface IVisitorAssignmentModifier
{
    string Describe();
    double Adjust(VisitorAssignment item, double valueValue);
}

public sealed record CapVisitorAssignmentAdjuster(double Cap) : IVisitorAssignmentModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(VisitorAssignment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingVisitorAssignmentAdjuster(double Step) : IVisitorAssignmentModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(VisitorAssignment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumVisitorAssignmentAdjuster(double Minimum) : IVisitorAssignmentModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(VisitorAssignment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the visitor assignment steps in order and keeps a trace of each.</summary>
public sealed class VisitorAssignmentEngine(List<IVisitorAssignmentModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IVisitorAssignmentModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static VisitorAssignmentEngine DefaultVisitorAssignmentEngine()
    {
        return new VisitorAssignmentEngine(
            new List<IVisitorAssignmentModifier>
            {
                new CapVisitorAssignmentAdjuster(18986.0),
                new RoundingVisitorAssignmentAdjuster(5.0),
                new MinimumVisitorAssignmentAdjuster(0.2),
            },
            new List<string>(),
            0.2,
            46917.0);
    }

    public double Compute(VisitorAssignment item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<VisitorAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
