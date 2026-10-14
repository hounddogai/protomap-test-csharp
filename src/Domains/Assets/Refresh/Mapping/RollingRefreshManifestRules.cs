namespace Acme.Workforce.Assets.Refresh.Mapping;

/// <summary>A rule a rolling refresh manifest must pass. Check returns its problems, or none.</summary>
public interface IRollingRefreshManifestRule
{
    List<string> Check(RollingRefreshManifest item);
}

public sealed record RollingRefreshManifestSkillLengthRule(int MaxLength) : IRollingRefreshManifestRule
{
    public List<string> Check(RollingRefreshManifest item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingRefreshManifestRequiredTitleRule(string Message) : IRollingRefreshManifestRule
{
    public List<string> Check(RollingRefreshManifest item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingRefreshManifestRequiredSkillRule(string Message) : IRollingRefreshManifestRule
{
    public List<string> Check(RollingRefreshManifest item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling refresh manifest.</summary>
public sealed record RollingRefreshManifestValidator(List<IRollingRefreshManifestRule> Rules)
{
    public List<string> Validate(RollingRefreshManifest item)
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

    public void RequireValid(RollingRefreshManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingRefreshManifest> items)
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

/// <summary>Functions of rolling refresh manifest rules.</summary>
public static class RollingRefreshManifestRules
{
    public static RollingRefreshManifestValidator DefaultRollingRefreshManifestValidator()
    {
        return new RollingRefreshManifestValidator(
            new List<IRollingRefreshManifestRule>
            {
                new RollingRefreshManifestSkillLengthRule(255),
                new RollingRefreshManifestRequiredTitleRule("rolling refresh manifest needs a value"),
                new RollingRefreshManifestRequiredSkillRule("rolling refresh manifest needs this field"),
            });
    }
}
