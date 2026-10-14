namespace Acme.Workforce.Compliance.Exception.Checkpoint;

/// <summary>A rule a exception quota must pass. Check returns its problems, or none.</summary>
public interface IExceptionQuotaRule
{
    List<string> Check(ExceptionQuota item);
}

public sealed record ExceptionQuotaCountryLengthRule(int MaxLength) : IExceptionQuotaRule
{
    public List<string> Check(ExceptionQuota item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionQuotaRequiredLabelRule(string Message) : IExceptionQuotaRule
{
    public List<string> Check(ExceptionQuota item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ExceptionQuotaRequiredCountryRule(string Message) : IExceptionQuotaRule
{
    public List<string> Check(ExceptionQuota item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ExceptionQuotaLabelLengthRule(int MaxLength) : IExceptionQuotaRule
{
    public List<string> Check(ExceptionQuota item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionQuotaTagsLimitRule(int MaxCount, List<string> Banned) : IExceptionQuotaRule
{
    public List<string> Check(ExceptionQuota item)
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

/// <summary>Runs every rule of a exception quota.</summary>
public sealed record ExceptionQuotaValidator(List<IExceptionQuotaRule> Rules)
{
    public List<string> Validate(ExceptionQuota item)
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

    public void RequireValid(ExceptionQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ExceptionQuota> items)
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

/// <summary>Functions of exception quota rules.</summary>
public static class ExceptionQuotaRules
{
    public static ExceptionQuotaValidator DefaultExceptionQuotaValidator()
    {
        return new ExceptionQuotaValidator(
            new List<IExceptionQuotaRule>
            {
                new ExceptionQuotaCountryLengthRule(120),
                new ExceptionQuotaRequiredLabelRule("exception quota needs this field"),
                new ExceptionQuotaRequiredCountryRule("exception quota needs this field"),
                new ExceptionQuotaLabelLengthRule(80),
                new ExceptionQuotaTagsLimitRule(80, new List<string> { "global" }),
            });
    }
}
