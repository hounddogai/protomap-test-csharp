namespace Acme.Workforce.Absence.Balance.Preference;

/// <summary>A rule a regional balance digest must pass. Check returns its problems, or none.</summary>
public interface IRegionalBalanceDigestRule
{
    List<string> Check(RegionalBalanceDigest item);
}

public sealed record RegionalBalanceDigestVisibleGuardRule(int Limit) : IRegionalBalanceDigestRule
{
    public List<string> Check(RegionalBalanceDigest item)
    {
        if (item.Visible && item.EndsAt > Limit)
        {
            return new List<string> { $"ends at {item.EndsAt} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record RegionalBalanceDigestSkillLengthRule(int MaxLength) : IRegionalBalanceDigestRule
{
    public List<string> Check(RegionalBalanceDigest item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalBalanceDigestLeaveDaysRangeRule(int Min, int Max) : IRegionalBalanceDigestRule
{
    public List<string> Check(RegionalBalanceDigest item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalBalanceDigestRequiredTeamRule(string Message) : IRegionalBalanceDigestRule
{
    public List<string> Check(RegionalBalanceDigest item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalBalanceDigestRequiredNameRule(string Message) : IRegionalBalanceDigestRule
{
    public List<string> Check(RegionalBalanceDigest item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional balance digest.</summary>
public sealed record RegionalBalanceDigestValidator(List<IRegionalBalanceDigestRule> Rules)
{
    public List<string> Validate(RegionalBalanceDigest item)
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

    public void RequireValid(RegionalBalanceDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalBalanceDigest> items)
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

/// <summary>Functions of regional balance digest rules.</summary>
public static class RegionalBalanceDigestRules
{
    public static RegionalBalanceDigestValidator DefaultRegionalBalanceDigestValidator()
    {
        return new RegionalBalanceDigestValidator(
            new List<IRegionalBalanceDigestRule>
            {
                new RegionalBalanceDigestVisibleGuardRule(32),
                new RegionalBalanceDigestSkillLengthRule(16),
                new RegionalBalanceDigestLeaveDaysRangeRule(16, 1000),
                new RegionalBalanceDigestRequiredTeamRule("regional balance digest needs an entry"),
                new RegionalBalanceDigestRequiredNameRule("regional balance digest needs a value"),
            });
    }
}
