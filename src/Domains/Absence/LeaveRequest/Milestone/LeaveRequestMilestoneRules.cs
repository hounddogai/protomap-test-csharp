namespace Acme.Workforce.Absence.LeaveRequest.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a leave request milestone must pass. Check returns its problems, or none.</summary>
public interface ILeaveRequestMilestoneRule
{
    List<string> Check(LeaveRequestMilestone item);
}

public sealed record LeaveRequestMilestoneCodeLengthRule(int MaxLength) : ILeaveRequestMilestoneRule
{
    public List<string> Check(LeaveRequestMilestone item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestMilestoneAllowedTitleRule(List<string> Allowed) : ILeaveRequestMilestoneRule
{
    public List<string> Check(LeaveRequestMilestone item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestMilestoneRequiredTitleRule(string Message) : ILeaveRequestMilestoneRule
{
    public List<string> Check(LeaveRequestMilestone item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestMilestoneCostRangeRule(double Min, double Max) : ILeaveRequestMilestoneRule
{
    public List<string> Check(LeaveRequestMilestone item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a leave request milestone.</summary>
public sealed record LeaveRequestMilestoneValidator(List<ILeaveRequestMilestoneRule> Rules)
{
    public List<string> Validate(LeaveRequestMilestone item)
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

    public void RequireValid(LeaveRequestMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LeaveRequestMilestone> items)
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

/// <summary>Functions of leave request milestone rules.</summary>
public static class LeaveRequestMilestoneRules
{
    public static LeaveRequestMilestoneValidator DefaultLeaveRequestMilestoneValidator()
    {
        return new LeaveRequestMilestoneValidator(
            new List<ILeaveRequestMilestoneRule>
            {
                new LeaveRequestMilestoneCodeLengthRule(80),
                new LeaveRequestMilestoneAllowedTitleRule(new List<string> { "primary", "pending", "primary" }),
                new LeaveRequestMilestoneRequiredTitleRule("leave request milestone needs an entry"),
                new LeaveRequestMilestoneCostRangeRule(0.5, 450.3),
            });
    }
}
