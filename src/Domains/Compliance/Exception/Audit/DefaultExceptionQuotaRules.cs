namespace Acme.Workforce.Compliance.Exception.Audit;

/// <summary>A rule a default exception quota must pass. Check returns its problems, or none.</summary>
public interface IDefaultExceptionQuotaRule
{
    List<string> Check(DefaultExceptionQuota item);
}

public sealed record DefaultExceptionQuotaRequiredCodeRule(string Message) : IDefaultExceptionQuotaRule
{
    public List<string> Check(DefaultExceptionQuota item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultExceptionQuotaTenureMonthsRangeRule(int Min, int Max) : IDefaultExceptionQuotaRule
{
    public List<string> Check(DefaultExceptionQuota item)
    {
        var valueValue = item.TenureMonths;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"tenure months {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultExceptionQuotaRequiredTitleRule(string Message) : IDefaultExceptionQuotaRule
{
    public List<string> Check(DefaultExceptionQuota item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default exception quota.</summary>
public sealed record DefaultExceptionQuotaValidator(List<IDefaultExceptionQuotaRule> Rules)
{
    public List<string> Validate(DefaultExceptionQuota item)
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

    public void RequireValid(DefaultExceptionQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultExceptionQuota> items)
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

/// <summary>Functions of default exception quota rules.</summary>
public static class DefaultExceptionQuotaRules
{
    public static DefaultExceptionQuotaValidator DefaultDefaultExceptionQuotaValidator()
    {
        return new DefaultExceptionQuotaValidator(
            new List<IDefaultExceptionQuotaRule>
            {
                new DefaultExceptionQuotaRequiredCodeRule("default exception quota needs this field"),
                new DefaultExceptionQuotaTenureMonthsRangeRule(16, 120),
                new DefaultExceptionQuotaRequiredTitleRule("default exception quota needs this field"),
            });
    }
}
