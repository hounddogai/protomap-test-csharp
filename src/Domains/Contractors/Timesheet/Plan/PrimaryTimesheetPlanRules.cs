namespace Acme.Workforce.Contractors.Timesheet.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a primary timesheet plan must pass. Check returns its problems, or none.</summary>
public interface IPrimaryTimesheetPlanRule
{
    List<string> Check(PrimaryTimesheetPlan item);
}

public sealed record PrimaryTimesheetPlanTimezoneLengthRule(int MaxLength) : IPrimaryTimesheetPlanRule
{
    public List<string> Check(PrimaryTimesheetPlan item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryTimesheetPlanAllowedTimezoneRule(List<string> Allowed) : IPrimaryTimesheetPlanRule
{
    public List<string> Check(PrimaryTimesheetPlan item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Timezone)))
        {
            return new List<string> { $"timezone {item.Timezone} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryTimesheetPlanRequiredCodeRule(string Message) : IPrimaryTimesheetPlanRule
{
    public List<string> Check(PrimaryTimesheetPlan item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryTimesheetPlanCodeLengthRule(int MaxLength) : IPrimaryTimesheetPlanRule
{
    public List<string> Check(PrimaryTimesheetPlan item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryTimesheetPlanRequiredGradeRule(string Message) : IPrimaryTimesheetPlanRule
{
    public List<string> Check(PrimaryTimesheetPlan item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary timesheet plan.</summary>
public sealed record PrimaryTimesheetPlanValidator(List<IPrimaryTimesheetPlanRule> Rules)
{
    public List<string> Validate(PrimaryTimesheetPlan item)
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

    public void RequireValid(PrimaryTimesheetPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryTimesheetPlan> items)
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

/// <summary>Functions of primary timesheet plan rules.</summary>
public static class PrimaryTimesheetPlanRules
{
    public static PrimaryTimesheetPlanValidator DefaultPrimaryTimesheetPlanValidator()
    {
        return new PrimaryTimesheetPlanValidator(
            new List<IPrimaryTimesheetPlanRule>
            {
                new PrimaryTimesheetPlanTimezoneLengthRule(1000),
                new PrimaryTimesheetPlanAllowedTimezoneRule(new List<string>()),
                new PrimaryTimesheetPlanRequiredCodeRule("primary timesheet plan needs this field"),
                new PrimaryTimesheetPlanCodeLengthRule(255),
                new PrimaryTimesheetPlanRequiredGradeRule("primary timesheet plan needs this field"),
            });
    }
}
