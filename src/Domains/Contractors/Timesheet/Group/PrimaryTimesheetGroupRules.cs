namespace Acme.Workforce.Contractors.Timesheet.Group;

/// <summary>A rule a primary timesheet group must pass. Check returns its problems, or none.</summary>
public interface IPrimaryTimesheetGroupRule
{
    List<string> Check(PrimaryTimesheetGroup item);
}

public sealed record PrimaryTimesheetGroupRequiredJobCodeRule(string Message) : IPrimaryTimesheetGroupRule
{
    public List<string> Check(PrimaryTimesheetGroup item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryTimesheetGroupOwnerIdLengthRule(int MaxLength) : IPrimaryTimesheetGroupRule
{
    public List<string> Check(PrimaryTimesheetGroup item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryTimesheetGroupWeightRangeRule(double Min, double Max) : IPrimaryTimesheetGroupRule
{
    public List<string> Check(PrimaryTimesheetGroup item)
    {
        var valueValue = item.Weight;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"weight {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"weight {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PrimaryTimesheetGroupJobCodeLengthRule(int MaxLength) : IPrimaryTimesheetGroupRule
{
    public List<string> Check(PrimaryTimesheetGroup item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryTimesheetGroupActiveGuardRule(int Limit) : IPrimaryTimesheetGroupRule
{
    public List<string> Check(PrimaryTimesheetGroup item)
    {
        if (item.Active && item.GraceDays > Limit)
        {
            return new List<string> { $"grace days {item.GraceDays} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryTimesheetGroupNameLengthRule(int MaxLength) : IPrimaryTimesheetGroupRule
{
    public List<string> Check(PrimaryTimesheetGroup item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary timesheet group.</summary>
public sealed record PrimaryTimesheetGroupValidator(List<IPrimaryTimesheetGroupRule> Rules)
{
    public List<string> Validate(PrimaryTimesheetGroup item)
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

    public void RequireValid(PrimaryTimesheetGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryTimesheetGroup> items)
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

/// <summary>Functions of primary timesheet group rules.</summary>
public static class PrimaryTimesheetGroupRules
{
    public static PrimaryTimesheetGroupValidator DefaultPrimaryTimesheetGroupValidator()
    {
        return new PrimaryTimesheetGroupValidator(
            new List<IPrimaryTimesheetGroupRule>
            {
                new PrimaryTimesheetGroupRequiredJobCodeRule("primary timesheet group needs a value"),
                new PrimaryTimesheetGroupOwnerIdLengthRule(3600),
                new PrimaryTimesheetGroupWeightRangeRule(3.4, 3496.0),
                new PrimaryTimesheetGroupJobCodeLengthRule(3600),
                new PrimaryTimesheetGroupActiveGuardRule(80),
                new PrimaryTimesheetGroupNameLengthRule(255),
            });
    }
}
