namespace Acme.Workforce.Talent.CareerPath.PathSummary;

/// <summary>A rule a regional career path quota must pass. Check returns its problems, or none.</summary>
public interface IRegionalCareerPathQuotaRule
{
    List<string> Check(RegionalCareerPathQuota item);
}

public sealed record RegionalCareerPathQuotaArchivedGuardRule(double Limit) : IRegionalCareerPathQuotaRule
{
    public List<string> Check(RegionalCareerPathQuota item)
    {
        if (item.Archived && item.Score > Limit)
        {
            return new List<string> { $"score {item.Score} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record RegionalCareerPathQuotaRequiredLocaleRule(string Message) : IRegionalCareerPathQuotaRule
{
    public List<string> Check(RegionalCareerPathQuota item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalCareerPathQuotaCodePrefixRule(string Prefix) : IRegionalCareerPathQuotaRule
{
    public List<string> Check(RegionalCareerPathQuota item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalCareerPathQuotaRequiredCodeRule(string Message) : IRegionalCareerPathQuotaRule
{
    public List<string> Check(RegionalCareerPathQuota item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalCareerPathQuotaRequiredCategoryRule(string Message) : IRegionalCareerPathQuotaRule
{
    public List<string> Check(RegionalCareerPathQuota item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalCareerPathQuotaRequiredLabelRule(string Message) : IRegionalCareerPathQuotaRule
{
    public List<string> Check(RegionalCareerPathQuota item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional career path quota.</summary>
public sealed record RegionalCareerPathQuotaValidator(List<IRegionalCareerPathQuotaRule> Rules)
{
    public List<string> Validate(RegionalCareerPathQuota item)
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

    public void RequireValid(RegionalCareerPathQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalCareerPathQuota> items)
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

/// <summary>Functions of regional career path quota rules.</summary>
public static class RegionalCareerPathQuotaRules
{
    public static RegionalCareerPathQuotaValidator DefaultRegionalCareerPathQuotaValidator()
    {
        return new RegionalCareerPathQuotaValidator(
            new List<IRegionalCareerPathQuotaRule>
            {
                new RegionalCareerPathQuotaArchivedGuardRule(5.0),
                new RegionalCareerPathQuotaRequiredLocaleRule("regional career path quota needs an entry"),
                new RegionalCareerPathQuotaCodePrefixRule("X"),
                new RegionalCareerPathQuotaRequiredCodeRule("regional career path quota needs this field"),
                new RegionalCareerPathQuotaRequiredCategoryRule("regional career path quota needs this field"),
                new RegionalCareerPathQuotaRequiredLabelRule("regional career path quota needs an entry"),
            });
    }
}
