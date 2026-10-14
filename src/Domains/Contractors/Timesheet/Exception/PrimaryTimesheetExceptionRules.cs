namespace Acme.Workforce.Contractors.Timesheet.Exception;

/// <summary>A rule a primary timesheet exception must pass. Check returns its problems, or none.</summary>
public interface IPrimaryTimesheetExceptionRule
{
    List<string> Check(PrimaryTimesheetException item);
}

public sealed record PrimaryTimesheetExceptionTagsLimitRule(int MaxCount, List<string> Banned) : IPrimaryTimesheetExceptionRule
{
    public List<string> Check(PrimaryTimesheetException item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

public sealed record PrimaryTimesheetExceptionCodeLengthRule(int MaxLength) : IPrimaryTimesheetExceptionRule
{
    public List<string> Check(PrimaryTimesheetException item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryTimesheetExceptionNameLengthRule(int MaxLength) : IPrimaryTimesheetExceptionRule
{
    public List<string> Check(PrimaryTimesheetException item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryTimesheetExceptionRequiredSourceRule(string Message) : IPrimaryTimesheetExceptionRule
{
    public List<string> Check(PrimaryTimesheetException item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary timesheet exception.</summary>
public sealed record PrimaryTimesheetExceptionValidator(List<IPrimaryTimesheetExceptionRule> Rules)
{
    public List<string> Validate(PrimaryTimesheetException item)
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

    public void RequireValid(PrimaryTimesheetException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryTimesheetException> items)
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

/// <summary>Functions of primary timesheet exception rules.</summary>
public static class PrimaryTimesheetExceptionRules
{
    public static PrimaryTimesheetExceptionValidator DefaultPrimaryTimesheetExceptionValidator()
    {
        return new PrimaryTimesheetExceptionValidator(
            new List<IPrimaryTimesheetExceptionRule>
            {
                new PrimaryTimesheetExceptionTagsLimitRule(64, new List<string> { "lenient", "legacy" }),
                new PrimaryTimesheetExceptionCodeLengthRule(80),
                new PrimaryTimesheetExceptionNameLengthRule(16),
                new PrimaryTimesheetExceptionRequiredSourceRule("primary timesheet exception needs a value"),
            });
    }
}
