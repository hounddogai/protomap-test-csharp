namespace Acme.Workforce.Engagement.PulseSurvey.Audit;

/// <summary>A rule a pulse survey audit must pass. Check returns its problems, or none.</summary>
public interface IPulseSurveyAuditRule
{
    List<string> Check(PulseSurveyAudit item);
}

public sealed record PulseSurveyAuditJobCodeLengthRule(int MaxLength) : IPulseSurveyAuditRule
{
    public List<string> Check(PulseSurveyAudit item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyAuditCodeLengthRule(int MaxLength) : IPulseSurveyAuditRule
{
    public List<string> Check(PulseSurveyAudit item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyAuditLabelLengthRule(int MaxLength) : IPulseSurveyAuditRule
{
    public List<string> Check(PulseSurveyAudit item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyAuditActiveGuardRule(int Limit) : IPulseSurveyAuditRule
{
    public List<string> Check(PulseSurveyAudit item)
    {
        if (item.Active && item.WindowDays > Limit)
        {
            return new List<string> { $"window days {item.WindowDays} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyAuditRequiredLabelRule(string Message) : IPulseSurveyAuditRule
{
    public List<string> Check(PulseSurveyAudit item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pulse survey audit.</summary>
public sealed record PulseSurveyAuditValidator(List<IPulseSurveyAuditRule> Rules)
{
    public List<string> Validate(PulseSurveyAudit item)
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

    public void RequireValid(PulseSurveyAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PulseSurveyAudit> items)
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

/// <summary>Functions of pulse survey audit rules.</summary>
public static class PulseSurveyAuditRules
{
    public static PulseSurveyAuditValidator DefaultPulseSurveyAuditValidator()
    {
        return new PulseSurveyAuditValidator(
            new List<IPulseSurveyAuditRule>
            {
                new PulseSurveyAuditJobCodeLengthRule(16),
                new PulseSurveyAuditCodeLengthRule(120),
                new PulseSurveyAuditLabelLengthRule(120),
                new PulseSurveyAuditActiveGuardRule(80),
                new PulseSurveyAuditRequiredLabelRule("pulse survey audit needs an entry"),
            });
    }
}
