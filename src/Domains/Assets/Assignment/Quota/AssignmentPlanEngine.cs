namespace Acme.Workforce.Assets.Assignment.Quota;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the assignment plan calculation.</summary>
public interface IAssignmentPlanFactor
{
    string Describe();
    double Adjust(AssignmentPlan item, double valueValue);
}

public sealed record MinimumAssignmentPlanAdjuster(double Minimum) : IAssignmentPlanFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AssignmentPlan item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapAssignmentPlanAdjuster(double Cap) : IAssignmentPlanFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AssignmentPlan item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingAssignmentPlanAdjuster(double Step) : IAssignmentPlanFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AssignmentPlan item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record AssignmentPlanScoreOffsetAdjuster(double Weight) : IAssignmentPlanFactor
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(AssignmentPlan item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

/// <summary>Runs the assignment plan steps in order and keeps a trace of each.</summary>
public sealed class AssignmentPlanEngine(List<IAssignmentPlanFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAssignmentPlanFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AssignmentPlanEngine DefaultAssignmentPlanEngine()
    {
        return new AssignmentPlanEngine(
            new List<IAssignmentPlanFactor>
            {
                new MinimumAssignmentPlanAdjuster(36.6),
                new CapAssignmentPlanAdjuster(33158.0),
                new RoundingAssignmentPlanAdjuster(1.0),
                new AssignmentPlanScoreOffsetAdjuster(0.28),
            },
            new List<string>(),
            1.1,
            28624.9);
    }

    public double Evaluate(AssignmentPlan item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<AssignmentPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
