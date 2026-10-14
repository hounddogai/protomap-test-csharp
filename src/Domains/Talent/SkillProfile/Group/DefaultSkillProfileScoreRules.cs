namespace Acme.Workforce.Talent.SkillProfile.Group;

/// <summary>A rule a default skill profile score must pass. Check returns its problems, or none.</summary>
public interface IDefaultSkillProfileScoreRule
{
    List<string> Check(DefaultSkillProfileScore item);
}

public sealed record DefaultSkillProfileScoreExpiresAtRangeRule(int Min, int Max) : IDefaultSkillProfileScoreRule
{
    public List<string> Check(DefaultSkillProfileScore item)
    {
        var valueValue = item.ExpiresAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"expires at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"expires at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DefaultSkillProfileScoreSkillLengthRule(int MaxLength) : IDefaultSkillProfileScoreRule
{
    public List<string> Check(DefaultSkillProfileScore item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultSkillProfileScoreCodeLengthRule(int MaxLength) : IDefaultSkillProfileScoreRule
{
    public List<string> Check(DefaultSkillProfileScore item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultSkillProfileScoreTitleLengthRule(int MaxLength) : IDefaultSkillProfileScoreRule
{
    public List<string> Check(DefaultSkillProfileScore item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default skill profile score.</summary>
public sealed record DefaultSkillProfileScoreValidator(List<IDefaultSkillProfileScoreRule> Rules)
{
    public List<string> Validate(DefaultSkillProfileScore item)
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

    public void RequireValid(DefaultSkillProfileScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultSkillProfileScore> items)
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

/// <summary>Functions of default skill profile score rules.</summary>
public static class DefaultSkillProfileScoreRules
{
    public static DefaultSkillProfileScoreValidator DefaultDefaultSkillProfileScoreValidator()
    {
        return new DefaultSkillProfileScoreValidator(
            new List<IDefaultSkillProfileScoreRule>
            {
                new DefaultSkillProfileScoreExpiresAtRangeRule(64, 255),
                new DefaultSkillProfileScoreSkillLengthRule(1000),
                new DefaultSkillProfileScoreCodeLengthRule(1000),
                new DefaultSkillProfileScoreTitleLengthRule(255),
            });
    }
}
