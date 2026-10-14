namespace Acme.Workforce.Absence.LeaveRequest.Budget;

/// <summary>One step of the leave request rule calculation.</summary>
public interface ILeaveRequestRuleFactor
{
    string Describe();
    double Adjust(LeaveRequestRule item, double valueValue);
}

public sealed record LeaveRequestRuleAmountOffsetAdjuster(double Weight) : ILeaveRequestRuleFactor
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(LeaveRequestRule item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

public sealed record MinimumLeaveRequestRuleAdjuster(double Minimum) : ILeaveRequestRuleFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LeaveRequestRule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapLeaveRequestRuleAdjuster(double Cap) : ILeaveRequestRuleFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LeaveRequestRule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record CountryLeaveRequestRuleAdjuster(Dictionary<string, double> Factors) : ILeaveRequestRuleFactor
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(LeaveRequestRule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the leave request rule steps in order and keeps a trace of each.</summary>
public sealed class LeaveRequestRuleEngine(List<ILeaveRequestRuleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILeaveRequestRuleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LeaveRequestRuleEngine DefaultLeaveRequestRuleEngine()
    {
        return new LeaveRequestRuleEngine(
            new List<ILeaveRequestRuleFactor>
            {
                new LeaveRequestRuleAmountOffsetAdjuster(0.91),
                new MinimumLeaveRequestRuleAdjuster(23.9),
                new CapLeaveRequestRuleAdjuster(40929.0),
                new CountryLeaveRequestRuleAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.05, ["west"] = 0.91 }),
            },
            new List<string>(),
            1.3,
            45039.4);
    }

    public double Compute(LeaveRequestRule item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<LeaveRequestRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
