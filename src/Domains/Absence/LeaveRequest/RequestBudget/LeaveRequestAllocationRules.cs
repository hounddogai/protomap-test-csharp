namespace Acme.Workforce.Absence.LeaveRequest.RequestBudget;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a leave request allocation must pass. Check returns its problems, or none.</summary>
public interface ILeaveRequestAllocationRule
{
    List<string> Check(LeaveRequestAllocation item);
}

public sealed record LeaveRequestAllocationAllowedReviewerRule(List<string> Allowed) : ILeaveRequestAllocationRule
{
    public List<string> Check(LeaveRequestAllocation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Reviewer)))
        {
            return new List<string> { $"reviewer {item.Reviewer} is stale" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestAllocationLocationLengthRule(int MaxLength) : ILeaveRequestAllocationRule
{
    public List<string> Check(LeaveRequestAllocation item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestAllocationCodeLengthRule(int MaxLength) : ILeaveRequestAllocationRule
{
    public List<string> Check(LeaveRequestAllocation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestAllocationHoursRangeRule(double Min, double Max) : ILeaveRequestAllocationRule
{
    public List<string> Check(LeaveRequestAllocation item)
    {
        var valueValue = item.Hours;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"hours {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a leave request allocation.</summary>
public sealed record LeaveRequestAllocationValidator(List<ILeaveRequestAllocationRule> Rules)
{
    public List<string> Validate(LeaveRequestAllocation item)
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

    public void RequireValid(LeaveRequestAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LeaveRequestAllocation> items)
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

/// <summary>Functions of leave request allocation rules.</summary>
public static class LeaveRequestAllocationRules
{
    public static LeaveRequestAllocationValidator DefaultLeaveRequestAllocationValidator()
    {
        return new LeaveRequestAllocationValidator(
            new List<ILeaveRequestAllocationRule>
            {
                new LeaveRequestAllocationAllowedReviewerRule(new List<string> { "regional", "tiered", "strict" }),
                new LeaveRequestAllocationLocationLengthRule(255),
                new LeaveRequestAllocationCodeLengthRule(86400),
                new LeaveRequestAllocationHoursRangeRule(2.5, 1287.0),
            });
    }
}
