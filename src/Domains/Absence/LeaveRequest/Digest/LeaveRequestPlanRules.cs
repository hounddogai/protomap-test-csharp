namespace Acme.Workforce.Absence.LeaveRequest.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a leave request plan must pass. Check returns its problems, or none.</summary>
public interface ILeaveRequestPlanRule
{
    List<string> Check(LeaveRequestPlan item);
}

public sealed record LeaveRequestPlanCodePrefixRule(string Prefix) : ILeaveRequestPlanRule
{
    public List<string> Check(LeaveRequestPlan item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestPlanCapacityRangeRule(int Min, int Max) : ILeaveRequestPlanRule
{
    public List<string> Check(LeaveRequestPlan item)
    {
        var valueValue = item.Capacity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"capacity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestPlanAllowedCodeRule(List<string> Allowed) : ILeaveRequestPlanRule
{
    public List<string> Check(LeaveRequestPlan item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is blocked" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a leave request plan.</summary>
public sealed record LeaveRequestPlanValidator(List<ILeaveRequestPlanRule> Rules)
{
    public List<string> Validate(LeaveRequestPlan item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            foreach (var problem in rule.Check(item))
            {
                problems.Add(problem);
            }
        }
        return problems;
    }

    public void RequireValid(LeaveRequestPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LeaveRequestPlan> items)
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

/// <summary>Functions of leave request plan rules.</summary>
public static class LeaveRequestPlanRules
{
    public static LeaveRequestPlanValidator DefaultLeaveRequestPlanValidator()
    {
        return new LeaveRequestPlanValidator(
            new List<ILeaveRequestPlanRule>
            {
                new LeaveRequestPlanCodePrefixRule("X"),
                new LeaveRequestPlanCapacityRangeRule(32, 255),
                new LeaveRequestPlanAllowedCodeRule(new List<string> { "fallback", "primary" }),
            });
    }
}
