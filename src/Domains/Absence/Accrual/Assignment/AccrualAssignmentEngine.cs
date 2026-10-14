namespace Acme.Workforce.Absence.Accrual.Assignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the accrual assignment calculation.</summary>
public interface IAccrualAssignmentFactor
{
    string Describe();
    double Adjust(AccrualAssignment item, double valueValue);
}

public sealed record RoundingAccrualAssignmentAdjuster(double Step) : IAccrualAssignmentFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AccrualAssignment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumAccrualAssignmentAdjuster(double Minimum) : IAccrualAssignmentFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AccrualAssignment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TitleAccrualAssignmentAdjuster(Dictionary<string, double> Factors) : IAccrualAssignmentFactor
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(AccrualAssignment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentAccrualAssignmentAdjuster(double Percent) : IAccrualAssignmentFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AccrualAssignment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the accrual assignment steps in order and keeps a trace of each.</summary>
public sealed class AccrualAssignmentEngine(List<IAccrualAssignmentFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccrualAssignmentFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccrualAssignmentEngine DefaultAccrualAssignmentEngine()
    {
        return new AccrualAssignmentEngine(
            new List<IAccrualAssignmentFactor>
            {
                new RoundingAccrualAssignmentAdjuster(5.0),
                new MinimumAccrualAssignmentAdjuster(13.8),
                new TitleAccrualAssignmentAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.95, ["retail"] = 0.9, ["north"] = 1.03 }),
                new PercentAccrualAssignmentAdjuster(13.3),
            },
            new List<string>(),
            2.1,
            70507.6);
    }

    public double Evaluate(AccrualAssignment item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<AccrualAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
