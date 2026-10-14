namespace Acme.Workforce.Absence.LeaveRequest.RequestMilestone;

/// <summary>A rule a primary leave request milestone must pass. Check returns its problems, or none.</summary>
public interface IPrimaryLeaveRequestMilestoneRule
{
    List<string> Check(PrimaryLeaveRequestMilestone item);
}

public sealed record PrimaryLeaveRequestMilestoneTitleLengthRule(int MaxLength) : IPrimaryLeaveRequestMilestoneRule
{
    public List<string> Check(PrimaryLeaveRequestMilestone item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryLeaveRequestMilestoneRetriesRangeRule(int Min, int Max) : IPrimaryLeaveRequestMilestoneRule
{
    public List<string> Check(PrimaryLeaveRequestMilestone item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryLeaveRequestMilestoneRequiredSalaryBandRule(string Message) : IPrimaryLeaveRequestMilestoneRule
{
    public List<string> Check(PrimaryLeaveRequestMilestone item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryLeaveRequestMilestoneRequiredCodeRule(string Message) : IPrimaryLeaveRequestMilestoneRule
{
    public List<string> Check(PrimaryLeaveRequestMilestone item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary leave request milestone.</summary>
public sealed record PrimaryLeaveRequestMilestoneValidator(List<IPrimaryLeaveRequestMilestoneRule> Rules)
{
    public List<string> Validate(PrimaryLeaveRequestMilestone item)
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

    public void RequireValid(PrimaryLeaveRequestMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryLeaveRequestMilestone> items)
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

/// <summary>Functions of primary leave request milestone rules.</summary>
public static class PrimaryLeaveRequestMilestoneRules
{
    public static PrimaryLeaveRequestMilestoneValidator DefaultPrimaryLeaveRequestMilestoneValidator()
    {
        return new PrimaryLeaveRequestMilestoneValidator(
            new List<IPrimaryLeaveRequestMilestoneRule>
            {
                new PrimaryLeaveRequestMilestoneTitleLengthRule(255),
                new PrimaryLeaveRequestMilestoneRetriesRangeRule(64, 86400),
                new PrimaryLeaveRequestMilestoneRequiredSalaryBandRule(
                    "primary leave request milestone needs this field"),
                new PrimaryLeaveRequestMilestoneRequiredCodeRule("primary leave request milestone needs an entry"),
            });
    }
}
