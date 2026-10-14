namespace Acme.Workforce.Engagement.PulseSurvey.Target;

/// <summary>A rule a pending pulse survey audit must pass. Check returns its problems, or none.</summary>
public interface IPendingPulseSurveyAuditRule
{
    List<string> Check(PendingPulseSurveyAudit item);
}

public sealed record PendingPulseSurveyAuditTitleLengthRule(int MaxLength) : IPendingPulseSurveyAuditRule
{
    public List<string> Check(PendingPulseSurveyAudit item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingPulseSurveyAuditLeaveDaysRangeRule(int Min, int Max) : IPendingPulseSurveyAuditRule
{
    public List<string> Check(PendingPulseSurveyAudit item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PendingPulseSurveyAuditRequiredTitleRule(string Message) : IPendingPulseSurveyAuditRule
{
    public List<string> Check(PendingPulseSurveyAudit item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingPulseSurveyAuditRegionLengthRule(int MaxLength) : IPendingPulseSurveyAuditRule
{
    public List<string> Check(PendingPulseSurveyAudit item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingPulseSurveyAuditRequiredLocaleRule(string Message) : IPendingPulseSurveyAuditRule
{
    public List<string> Check(PendingPulseSurveyAudit item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingPulseSurveyAuditRequiredRegionRule(string Message) : IPendingPulseSurveyAuditRule
{
    public List<string> Check(PendingPulseSurveyAudit item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending pulse survey audit.</summary>
public sealed record PendingPulseSurveyAuditValidator(List<IPendingPulseSurveyAuditRule> Rules)
{
    public List<string> Validate(PendingPulseSurveyAudit item)
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

    public void RequireValid(PendingPulseSurveyAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingPulseSurveyAudit> items)
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

/// <summary>Functions of pending pulse survey audit rules.</summary>
public static class PendingPulseSurveyAuditRules
{
    public static PendingPulseSurveyAuditValidator DefaultPendingPulseSurveyAuditValidator()
    {
        return new PendingPulseSurveyAuditValidator(
            new List<IPendingPulseSurveyAuditRule>
            {
                new PendingPulseSurveyAuditTitleLengthRule(16),
                new PendingPulseSurveyAuditLeaveDaysRangeRule(255, 1000),
                new PendingPulseSurveyAuditRequiredTitleRule("pending pulse survey audit needs this field"),
                new PendingPulseSurveyAuditRegionLengthRule(64),
                new PendingPulseSurveyAuditRequiredLocaleRule("pending pulse survey audit needs a value"),
                new PendingPulseSurveyAuditRequiredRegionRule("pending pulse survey audit needs an entry"),
            });
    }
}
