namespace Acme.Workforce.Absence.LeaveRequest.Plan;

/// <summary>A rule a manual leave request plan must pass. Check returns its problems, or none.</summary>
public interface IManualLeaveRequestPlanRule
{
    List<string> Check(ManualLeaveRequestPlan item);
}

public sealed record ManualLeaveRequestPlanWindowDaysRangeRule(int Min, int Max) : IManualLeaveRequestPlanRule
{
    public List<string> Check(ManualLeaveRequestPlan item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ManualLeaveRequestPlanRequiredCodeRule(string Message) : IManualLeaveRequestPlanRule
{
    public List<string> Check(ManualLeaveRequestPlan item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualLeaveRequestPlanAmountRangeRule(double Min, double Max) : IManualLeaveRequestPlanRule
{
    public List<string> Check(ManualLeaveRequestPlan item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual leave request plan.</summary>
public sealed record ManualLeaveRequestPlanValidator(List<IManualLeaveRequestPlanRule> Rules)
{
    public List<string> Validate(ManualLeaveRequestPlan item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            var found = rule.Check(item);
            if (found.Count == 0)
            {
                continue;
            }
            foreach (var problem in found)
            {
                problems.Add($"{item.Code}: {problem}");
            }
        }
        return problems;
    }

    public void RequireValid(ManualLeaveRequestPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualLeaveRequestPlan> items)
    {
        var report = new Dictionary<string, List<string>>();
        foreach (var item in items)
        {
            var problems = Validate(item);
            if (problems.Count > 0)
            {
                report[item.Code] = problems;
            }
        }
        return report;
    }
}

/// <summary>Functions of manual leave request plan rules.</summary>
public static class ManualLeaveRequestPlanRules
{
    public static ManualLeaveRequestPlanValidator DefaultManualLeaveRequestPlanValidator()
    {
        return new ManualLeaveRequestPlanValidator(
            new List<IManualLeaveRequestPlanRule>
            {
                new ManualLeaveRequestPlanWindowDaysRangeRule(32, 86400),
                new ManualLeaveRequestPlanRequiredCodeRule("manual leave request plan needs this field"),
                new ManualLeaveRequestPlanAmountRangeRule(4.5, 3657.1),
            });
    }
}
