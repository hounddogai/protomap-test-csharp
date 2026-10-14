namespace Acme.Workforce.Talent.SkillProfile.Score;

/// <summary>A rule a skill profile rollup must pass. Check returns its problems, or none.</summary>
public interface ISkillProfileRollupRule
{
    List<string> Check(SkillProfileRollup item);
}

public sealed record SkillProfileRollupArchivedGuardRule(int Limit) : ISkillProfileRollupRule
{
    public List<string> Check(SkillProfileRollup item)
    {
        if (item.Archived && item.StartsAt > Limit)
        {
            return new List<string> { $"starts at {item.StartsAt} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileRollupTitleLengthRule(int MaxLength) : ISkillProfileRollupRule
{
    public List<string> Check(SkillProfileRollup item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileRollupRequiredSummaryRule(string Message) : ISkillProfileRollupRule
{
    public List<string> Check(SkillProfileRollup item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a skill profile rollup.</summary>
public sealed record SkillProfileRollupValidator(List<ISkillProfileRollupRule> Rules)
{
    public List<string> Validate(SkillProfileRollup item)
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

    public void RequireValid(SkillProfileRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SkillProfileRollup> items)
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

/// <summary>Functions of skill profile rollup rules.</summary>
public static class SkillProfileRollupRules
{
    public static SkillProfileRollupValidator DefaultSkillProfileRollupValidator()
    {
        return new SkillProfileRollupValidator(
            new List<ISkillProfileRollupRule>
            {
                new SkillProfileRollupArchivedGuardRule(120),
                new SkillProfileRollupTitleLengthRule(16),
                new SkillProfileRollupRequiredSummaryRule("skill profile rollup needs this field"),
            });
    }
}
