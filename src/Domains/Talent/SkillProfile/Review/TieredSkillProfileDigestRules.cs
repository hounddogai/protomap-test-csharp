namespace Acme.Workforce.Talent.SkillProfile.Review;

/// <summary>A rule a tiered skill profile digest must pass. Check returns its problems, or none.</summary>
public interface ITieredSkillProfileDigestRule
{
    List<string> Check(TieredSkillProfileDigest item);
}

public sealed record TieredSkillProfileDigestCostCenterLengthRule(int MaxLength) : ITieredSkillProfileDigestRule
{
    public List<string> Check(TieredSkillProfileDigest item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredSkillProfileDigestRequiredCostCenterRule(string Message) : ITieredSkillProfileDigestRule
{
    public List<string> Check(TieredSkillProfileDigest item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredSkillProfileDigestRequiredCodeRule(string Message) : ITieredSkillProfileDigestRule
{
    public List<string> Check(TieredSkillProfileDigest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered skill profile digest.</summary>
public sealed record TieredSkillProfileDigestValidator(List<ITieredSkillProfileDigestRule> Rules)
{
    public List<string> Validate(TieredSkillProfileDigest item)
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

    public void RequireValid(TieredSkillProfileDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredSkillProfileDigest> items)
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

/// <summary>Functions of tiered skill profile digest rules.</summary>
public static class TieredSkillProfileDigestRules
{
    public static TieredSkillProfileDigestValidator DefaultTieredSkillProfileDigestValidator()
    {
        return new TieredSkillProfileDigestValidator(
            new List<ITieredSkillProfileDigestRule>
            {
                new TieredSkillProfileDigestCostCenterLengthRule(120),
                new TieredSkillProfileDigestRequiredCostCenterRule("tiered skill profile digest needs this field"),
                new TieredSkillProfileDigestRequiredCodeRule("tiered skill profile digest needs an entry"),
            });
    }
}
