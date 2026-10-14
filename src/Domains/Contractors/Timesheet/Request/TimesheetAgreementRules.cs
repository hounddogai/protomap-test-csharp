namespace Acme.Workforce.Contractors.Timesheet.Request;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a timesheet agreement must pass. Check returns its problems, or none.</summary>
public interface ITimesheetAgreementRule
{
    List<string> Check(TimesheetAgreement item);
}

public sealed record TimesheetAgreementPriorityRangeRule(int Min, int Max) : ITimesheetAgreementRule
{
    public List<string> Check(TimesheetAgreement item)
    {
        var valueValue = item.Priority;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"priority {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetAgreementCategoryLengthRule(int MaxLength) : ITimesheetAgreementRule
{
    public List<string> Check(TimesheetAgreement item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetAgreementRequiredLocaleRule(string Message) : ITimesheetAgreementRule
{
    public List<string> Check(TimesheetAgreement item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TimesheetAgreementAllowedCategoryRule(List<string> Allowed) : ITimesheetAgreementRule
{
    public List<string> Check(TimesheetAgreement item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Category)))
        {
            return new List<string> { $"category {item.Category} is expired" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetAgreementRequiredLabelRule(string Message) : ITimesheetAgreementRule
{
    public List<string> Check(TimesheetAgreement item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TimesheetAgreementLocaleLengthRule(int MaxLength) : ITimesheetAgreementRule
{
    public List<string> Check(TimesheetAgreement item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a timesheet agreement.</summary>
public sealed record TimesheetAgreementValidator(List<ITimesheetAgreementRule> Rules)
{
    public List<string> Validate(TimesheetAgreement item)
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

    public void RequireValid(TimesheetAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TimesheetAgreement> items)
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

/// <summary>Functions of timesheet agreement rules.</summary>
public static class TimesheetAgreementRules
{
    public static TimesheetAgreementValidator DefaultTimesheetAgreementValidator()
    {
        return new TimesheetAgreementValidator(
            new List<ITimesheetAgreementRule>
            {
                new TimesheetAgreementPriorityRangeRule(16, 120),
                new TimesheetAgreementCategoryLengthRule(80),
                new TimesheetAgreementRequiredLocaleRule("timesheet agreement needs this field"),
                new TimesheetAgreementAllowedCategoryRule(new List<string> { "scheduled", "regional", "lenient" }),
                new TimesheetAgreementRequiredLabelRule("timesheet agreement needs a value"),
                new TimesheetAgreementLocaleLengthRule(1000),
            });
    }
}
