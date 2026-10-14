namespace Acme.Workforce.Engagement.PulseSurvey.Profile;

/// <summary>A rule a seasonal pulse survey audit must pass. Check returns its problems, or none.</summary>
public interface ISeasonalPulseSurveyAuditRule
{
    List<string> Check(SeasonalPulseSurveyAudit item);
}

public sealed record SeasonalPulseSurveyAuditRequiredChecksumRule(string Message) : ISeasonalPulseSurveyAuditRule
{
    public List<string> Check(SeasonalPulseSurveyAudit item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalPulseSurveyAuditLeaveDaysRangeRule(int Min, int Max) : ISeasonalPulseSurveyAuditRule
{
    public List<string> Check(SeasonalPulseSurveyAudit item)
    {
        var valueValue = item.LeaveDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"leave days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"leave days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SeasonalPulseSurveyAuditRequiredCodeRule(string Message) : ISeasonalPulseSurveyAuditRule
{
    public List<string> Check(SeasonalPulseSurveyAudit item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalPulseSurveyAuditVisibleGuardRule(int Limit) : ISeasonalPulseSurveyAuditRule
{
    public List<string> Check(SeasonalPulseSurveyAudit item)
    {
        if (item.Visible && item.LeaveDays > Limit)
        {
            return new List<string> { $"leave days {item.LeaveDays} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalPulseSurveyAuditChecksumLengthRule(int MaxLength) : ISeasonalPulseSurveyAuditRule
{
    public List<string> Check(SeasonalPulseSurveyAudit item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalPulseSurveyAuditCodeLengthRule(int MaxLength) : ISeasonalPulseSurveyAuditRule
{
    public List<string> Check(SeasonalPulseSurveyAudit item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal pulse survey audit.</summary>
public sealed record SeasonalPulseSurveyAuditValidator(List<ISeasonalPulseSurveyAuditRule> Rules)
{
    public List<string> Validate(SeasonalPulseSurveyAudit item)
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

    public void RequireValid(SeasonalPulseSurveyAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalPulseSurveyAudit> items)
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

/// <summary>Functions of seasonal pulse survey audit rules.</summary>
public static class SeasonalPulseSurveyAuditRules
{
    public static SeasonalPulseSurveyAuditValidator DefaultSeasonalPulseSurveyAuditValidator()
    {
        return new SeasonalPulseSurveyAuditValidator(
            new List<ISeasonalPulseSurveyAuditRule>
            {
                new SeasonalPulseSurveyAuditRequiredChecksumRule("seasonal pulse survey audit needs a value"),
                new SeasonalPulseSurveyAuditLeaveDaysRangeRule(16, 3600),
                new SeasonalPulseSurveyAuditRequiredCodeRule("seasonal pulse survey audit needs a value"),
                new SeasonalPulseSurveyAuditVisibleGuardRule(3600),
                new SeasonalPulseSurveyAuditChecksumLengthRule(80),
                new SeasonalPulseSurveyAuditCodeLengthRule(3600),
            });
    }
}
