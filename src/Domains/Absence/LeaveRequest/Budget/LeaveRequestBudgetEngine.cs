namespace Acme.Workforce.Absence.LeaveRequest.Budget;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the leave request budget calculation.</summary>
public interface ILeaveRequestBudgetFactor
{
    string Describe();
    double Adjust(LeaveRequestBudget item, double valueValue);
}

public sealed record LeaveRequestBudgetPriorityOffsetAdjuster(double Weight) : ILeaveRequestBudgetFactor
{
    public string Describe()
    {
        return $"plus priority times {Weight}";
    }

    public double Adjust(LeaveRequestBudget item, double valueValue)
    {
        return valueValue + (double)item.Priority * Weight;
    }
}

public sealed record TieredLeaveRequestBudgetAdjuster(List<double> Bounds, List<double> Rates) : ILeaveRequestBudgetFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LeaveRequestBudget item, double valueValue)
    {
        for (var index = 0; index < Bounds.Count; index++)
        {
            if (valueValue <= Bounds[index] && index < Rates.Count)
            {
                return valueValue * Rates[index];
            }
        }
        return valueValue;
    }
}

public sealed record PercentLeaveRequestBudgetAdjuster(double Percent) : ILeaveRequestBudgetFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LeaveRequestBudget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapLeaveRequestBudgetAdjuster(double Cap) : ILeaveRequestBudgetFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LeaveRequestBudget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingLeaveRequestBudgetAdjuster(double Step) : ILeaveRequestBudgetFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LeaveRequestBudget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the leave request budget steps in order and keeps a trace of each.</summary>
public sealed class LeaveRequestBudgetEngine(List<ILeaveRequestBudgetFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILeaveRequestBudgetFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LeaveRequestBudgetEngine DefaultLeaveRequestBudgetEngine()
    {
        return new LeaveRequestBudgetEngine(
            new List<ILeaveRequestBudgetFactor>
            {
                new LeaveRequestBudgetPriorityOffsetAdjuster(0.26),
                new TieredLeaveRequestBudgetAdjuster(
                    new List<double> { 2196.0, 2797.0, 6583.0 },
                    new List<double> { 1.13, 0.9, 1.08 }),
                new PercentLeaveRequestBudgetAdjuster(-2.1),
                new CapLeaveRequestBudgetAdjuster(24418.0),
                new RoundingLeaveRequestBudgetAdjuster(0.05),
            },
            new List<string>(),
            3.1,
            65289.0);
    }

    public double Price(LeaveRequestBudget item)
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

    public Dictionary<string, double> EvaluateAll(List<LeaveRequestBudget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
