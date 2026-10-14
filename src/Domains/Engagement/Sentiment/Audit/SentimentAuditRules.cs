namespace Acme.Workforce.Engagement.Sentiment.Audit;

/// <summary>A rule a sentiment audit must pass. Check returns its problems, or none.</summary>
public interface ISentimentAuditRule
{
    List<string> Check(SentimentAudit item);
}

public sealed record SentimentAuditRequiredCodeRule(string Message) : ISentimentAuditRule
{
    public List<string> Check(SentimentAudit item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SentimentAuditLabelLengthRule(int MaxLength) : ISentimentAuditRule
{
    public List<string> Check(SentimentAudit item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentAuditRequiredDepartmentRule(string Message) : ISentimentAuditRule
{
    public List<string> Check(SentimentAudit item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SentimentAuditRequiredLabelRule(string Message) : ISentimentAuditRule
{
    public List<string> Check(SentimentAudit item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SentimentAuditCodeLengthRule(int MaxLength) : ISentimentAuditRule
{
    public List<string> Check(SentimentAudit item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a sentiment audit.</summary>
public sealed record SentimentAuditValidator(List<ISentimentAuditRule> Rules)
{
    public List<string> Validate(SentimentAudit item)
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

    public void RequireValid(SentimentAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SentimentAudit> items)
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

/// <summary>Functions of sentiment audit rules.</summary>
public static class SentimentAuditRules
{
    public static SentimentAuditValidator DefaultSentimentAuditValidator()
    {
        return new SentimentAuditValidator(
            new List<ISentimentAuditRule>
            {
                new SentimentAuditRequiredCodeRule("sentiment audit needs this field"),
                new SentimentAuditLabelLengthRule(16),
                new SentimentAuditRequiredDepartmentRule("sentiment audit needs an entry"),
                new SentimentAuditRequiredLabelRule("sentiment audit needs an entry"),
                new SentimentAuditCodeLengthRule(32),
            });
    }
}
