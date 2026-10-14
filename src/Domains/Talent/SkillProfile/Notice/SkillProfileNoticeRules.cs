namespace Acme.Workforce.Talent.SkillProfile.Notice;

/// <summary>A rule a skill profile notice must pass. Check returns its problems, or none.</summary>
public interface ISkillProfileNoticeRule
{
    List<string> Check(SkillProfileNotice item);
}

public sealed record SkillProfileNoticeScoreRangeRule(double Min, double Max) : ISkillProfileNoticeRule
{
    public List<string> Check(SkillProfileNotice item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileNoticeCodePrefixRule(string Prefix) : ISkillProfileNoticeRule
{
    public List<string> Check(SkillProfileNotice item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileNoticeTagsLimitRule(int MaxCount, List<string> Banned) : ISkillProfileNoticeRule
{
    public List<string> Check(SkillProfileNotice item)
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

/// <summary>Runs every rule of a skill profile notice.</summary>
public sealed record SkillProfileNoticeValidator(List<ISkillProfileNoticeRule> Rules)
{
    public List<string> Validate(SkillProfileNotice item)
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

    public void RequireValid(SkillProfileNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SkillProfileNotice> items)
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

/// <summary>Functions of skill profile notice rules.</summary>
public static class SkillProfileNoticeRules
{
    public static SkillProfileNoticeValidator DefaultSkillProfileNoticeValidator()
    {
        return new SkillProfileNoticeValidator(
            new List<ISkillProfileNoticeRule>
            {
                new SkillProfileNoticeScoreRangeRule(3.0, 3855.4),
                new SkillProfileNoticeCodePrefixRule("OPS"),
                new SkillProfileNoticeTagsLimitRule(120, new List<string> { "tiered", "adaptive", "batched" }),
            });
    }
}
