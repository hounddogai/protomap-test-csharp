namespace Acme.Workforce.Absence.Accrual.PendingAccrualAssignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling accrual assignment calculation.</summary>
public interface IRollingAccrualAssignmentStep
{
    string Describe();
    double Adjust(RollingAccrualAssignment item, double valueValue);
}

public sealed record MinimumRollingAccrualAssignmentAdjuster(double Minimum) : IRollingAccrualAssignmentStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RollingAccrualAssignment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingRollingAccrualAssignmentAdjuster(double Step) : IRollingAccrualAssignmentStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RollingAccrualAssignment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record UnitRollingAccrualAssignmentAdjuster(Dictionary<string, double> Factors) : IRollingAccrualAssignmentStep
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(RollingAccrualAssignment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the rolling accrual assignment steps in order and keeps a trace of each.</summary>
public sealed class RollingAccrualAssignmentEngine(List<IRollingAccrualAssignmentStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingAccrualAssignmentStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingAccrualAssignmentEngine DefaultRollingAccrualAssignmentEngine()
    {
        return new RollingAccrualAssignmentEngine(
            new List<IRollingAccrualAssignmentStep>
            {
                new MinimumRollingAccrualAssignmentAdjuster(14.1),
                new RoundingRollingAccrualAssignmentAdjuster(0.25),
                new UnitRollingAccrualAssignmentAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.92, ["online"] = 1.26 }),
            },
            new List<string>(),
            4.6,
            65940.9);
    }

    public double Compute(RollingAccrualAssignment item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<RollingAccrualAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
