namespace Acme.Workforce.Talent.SkillProfile.Mapping;

/// <summary>A rule a capped skill profile mapping must pass. Check returns its problems, or none.</summary>
public interface ICappedSkillProfileMappingRule
{
    List<string> Check(CappedSkillProfileMapping item);
}

public sealed record CappedSkillProfileMappingRequiredCategoryRule(string Message) : ICappedSkillProfileMappingRule
{
    public List<string> Check(CappedSkillProfileMapping item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedSkillProfileMappingCodeLengthRule(int MaxLength) : ICappedSkillProfileMappingRule
{
    public List<string> Check(CappedSkillProfileMapping item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedSkillProfileMappingSkillLengthRule(int MaxLength) : ICappedSkillProfileMappingRule
{
    public List<string> Check(CappedSkillProfileMapping item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedSkillProfileMappingRequiredTitleRule(string Message) : ICappedSkillProfileMappingRule
{
    public List<string> Check(CappedSkillProfileMapping item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped skill profile mapping.</summary>
public sealed record CappedSkillProfileMappingValidator(List<ICappedSkillProfileMappingRule> Rules)
{
    public List<string> Validate(CappedSkillProfileMapping item)
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

    public void RequireValid(CappedSkillProfileMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedSkillProfileMapping> items)
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

/// <summary>Functions of capped skill profile mapping rules.</summary>
public static class CappedSkillProfileMappingRules
{
    public static CappedSkillProfileMappingValidator DefaultCappedSkillProfileMappingValidator()
    {
        return new CappedSkillProfileMappingValidator(
            new List<ICappedSkillProfileMappingRule>
            {
                new CappedSkillProfileMappingRequiredCategoryRule("capped skill profile mapping needs an entry"),
                new CappedSkillProfileMappingCodeLengthRule(64),
                new CappedSkillProfileMappingSkillLengthRule(32),
                new CappedSkillProfileMappingRequiredTitleRule("capped skill profile mapping needs a value"),
            });
    }
}
