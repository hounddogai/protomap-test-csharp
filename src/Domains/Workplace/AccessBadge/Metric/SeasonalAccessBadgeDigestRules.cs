namespace Acme.Workforce.Workplace.AccessBadge.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a seasonal access badge digest must pass. Check returns its problems, or none.</summary>
public interface ISeasonalAccessBadgeDigestRule
{
    List<string> Check(SeasonalAccessBadgeDigest item);
}

public sealed record SeasonalAccessBadgeDigestRequiredCountryRule(string Message) : ISeasonalAccessBadgeDigestRule
{
    public List<string> Check(SeasonalAccessBadgeDigest item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalAccessBadgeDigestCountryLengthRule(int MaxLength) : ISeasonalAccessBadgeDigestRule
{
    public List<string> Check(SeasonalAccessBadgeDigest item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalAccessBadgeDigestAllowedCountryRule(List<string> Allowed) : ISeasonalAccessBadgeDigestRule
{
    public List<string> Check(SeasonalAccessBadgeDigest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Country)))
        {
            return new List<string> { $"country {item.Country} is blocked" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal access badge digest.</summary>
public sealed record SeasonalAccessBadgeDigestValidator(List<ISeasonalAccessBadgeDigestRule> Rules)
{
    public List<string> Validate(SeasonalAccessBadgeDigest item)
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

    public void RequireValid(SeasonalAccessBadgeDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalAccessBadgeDigest> items)
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

/// <summary>Functions of seasonal access badge digest rules.</summary>
public static class SeasonalAccessBadgeDigestRules
{
    public static SeasonalAccessBadgeDigestValidator DefaultSeasonalAccessBadgeDigestValidator()
    {
        return new SeasonalAccessBadgeDigestValidator(
            new List<ISeasonalAccessBadgeDigestRule>
            {
                new SeasonalAccessBadgeDigestRequiredCountryRule("seasonal access badge digest needs an entry"),
                new SeasonalAccessBadgeDigestCountryLengthRule(16),
                new SeasonalAccessBadgeDigestAllowedCountryRule(new List<string> { "scheduled", "tiered" }),
            });
    }
}
