namespace Acme.Workforce.Contractors.Timesheet.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a capped timesheet group must pass. Check returns its problems, or none.</summary>
public interface ICappedTimesheetGroupRule
{
    List<string> Check(CappedTimesheetGroup item);
}

public sealed record CappedTimesheetGroupAllowedChannelRule(List<string> Allowed) : ICappedTimesheetGroupRule
{
    public List<string> Check(CappedTimesheetGroup item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Channel)))
        {
            return new List<string> { $"channel {item.Channel} is stale" };
        }
        return new List<string>();
    }
}

public sealed record CappedTimesheetGroupRequiredUnitRule(string Message) : ICappedTimesheetGroupRule
{
    public List<string> Check(CappedTimesheetGroup item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedTimesheetGroupNameLengthRule(int MaxLength) : ICappedTimesheetGroupRule
{
    public List<string> Check(CappedTimesheetGroup item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedTimesheetGroupRequiredCodeRule(string Message) : ICappedTimesheetGroupRule
{
    public List<string> Check(CappedTimesheetGroup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped timesheet group.</summary>
public sealed record CappedTimesheetGroupValidator(List<ICappedTimesheetGroupRule> Rules)
{
    public List<string> Validate(CappedTimesheetGroup item)
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

    public void RequireValid(CappedTimesheetGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedTimesheetGroup> items)
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

/// <summary>Functions of capped timesheet group rules.</summary>
public static class CappedTimesheetGroupRules
{
    public static CappedTimesheetGroupValidator DefaultCappedTimesheetGroupValidator()
    {
        return new CappedTimesheetGroupValidator(
            new List<ICappedTimesheetGroupRule>
            {
                new CappedTimesheetGroupAllowedChannelRule(new List<string>()),
                new CappedTimesheetGroupRequiredUnitRule("capped timesheet group needs a value"),
                new CappedTimesheetGroupNameLengthRule(16),
                new CappedTimesheetGroupRequiredCodeRule("capped timesheet group needs a value"),
            });
    }
}
