namespace Acme.Workforce.Talent.SkillProfile.Score;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a skill profile score must pass. Check returns its problems, or none.</summary>
public interface ISkillProfileScoreRule
{
    List<string> Check(SkillProfileScore item);
}

public sealed record SkillProfileScoreCodePrefixRule(string Prefix) : ISkillProfileScoreRule
{
    public List<string> Check(SkillProfileScore item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileScoreWeightRangeRule(double Min, double Max) : ISkillProfileScoreRule
{
    public List<string> Check(SkillProfileScore item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileScoreAllowedNameRule(List<string> Allowed) : ISkillProfileScoreRule
{
    public List<string> Check(SkillProfileScore item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is stale" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a skill profile score.</summary>
public sealed record SkillProfileScoreValidator(List<ISkillProfileScoreRule> Rules)
{
    public List<string> Validate(SkillProfileScore item)
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

    public void RequireValid(SkillProfileScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SkillProfileScore> items)
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

/// <summary>Functions of skill profile score rules.</summary>
public static class SkillProfileScoreRules
{
    public static SkillProfileScoreValidator DefaultSkillProfileScoreValidator()
    {
        return new SkillProfileScoreValidator(
            new List<ISkillProfileScoreRule>
            {
                new SkillProfileScoreCodePrefixRule("ACM"),
                new SkillProfileScoreWeightRangeRule(3.1, 4708.9),
                new SkillProfileScoreAllowedNameRule(new List<string> { "primary", "regional" }),
            });
    }
}
