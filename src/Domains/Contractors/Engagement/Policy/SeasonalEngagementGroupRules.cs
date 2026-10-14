namespace Acme.Workforce.Contractors.Engagement.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a seasonal engagement group must pass. Check returns its problems, or none.</summary>
public interface ISeasonalEngagementGroupRule
{
    List<string> Check(SeasonalEngagementGroup item);
}

public sealed record SeasonalEngagementGroupCodeLengthRule(int MaxLength) : ISeasonalEngagementGroupRule
{
    public List<string> Check(SeasonalEngagementGroup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalEngagementGroupAllowedNameRule(List<string> Allowed) : ISeasonalEngagementGroupRule
{
    public List<string> Check(SeasonalEngagementGroup item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is missing" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalEngagementGroupCodePrefixRule(string Prefix) : ISeasonalEngagementGroupRule
{
    public List<string> Check(SeasonalEngagementGroup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalEngagementGroupVisibleGuardRule(int Limit) : ISeasonalEngagementGroupRule
{
    public List<string> Check(SeasonalEngagementGroup item)
    {
        if (item.Visible && item.EndsAt > Limit)
        {
            return new List<string> { $"ends at {item.EndsAt} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal engagement group.</summary>
public sealed record SeasonalEngagementGroupValidator(List<ISeasonalEngagementGroupRule> Rules)
{
    public List<string> Validate(SeasonalEngagementGroup item)
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

    public void RequireValid(SeasonalEngagementGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalEngagementGroup> items)
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

/// <summary>Functions of seasonal engagement group rules.</summary>
public static class SeasonalEngagementGroupRules
{
    public static SeasonalEngagementGroupValidator DefaultSeasonalEngagementGroupValidator()
    {
        return new SeasonalEngagementGroupValidator(
            new List<ISeasonalEngagementGroupRule>
            {
                new SeasonalEngagementGroupCodeLengthRule(80),
                new SeasonalEngagementGroupAllowedNameRule(new List<string> { "scheduled", "strict" }),
                new SeasonalEngagementGroupCodePrefixRule("Q"),
                new SeasonalEngagementGroupVisibleGuardRule(120),
            });
    }
}
