namespace Acme.Workforce.Talent.SkillProfile.ProfileSummary;

/// <summary>A rule a default skill profile draft must pass. Check returns its problems, or none.</summary>
public interface IDefaultSkillProfileDraftRule
{
    List<string> Check(DefaultSkillProfileDraft item);
}

public sealed record DefaultSkillProfileDraftCodeLengthRule(int MaxLength) : IDefaultSkillProfileDraftRule
{
    public List<string> Check(DefaultSkillProfileDraft item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultSkillProfileDraftDepartmentLengthRule(int MaxLength) : IDefaultSkillProfileDraftRule
{
    public List<string> Check(DefaultSkillProfileDraft item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultSkillProfileDraftRequiredCodeRule(string Message) : IDefaultSkillProfileDraftRule
{
    public List<string> Check(DefaultSkillProfileDraft item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultSkillProfileDraftNameLengthRule(int MaxLength) : IDefaultSkillProfileDraftRule
{
    public List<string> Check(DefaultSkillProfileDraft item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default skill profile draft.</summary>
public sealed record DefaultSkillProfileDraftValidator(List<IDefaultSkillProfileDraftRule> Rules)
{
    public List<string> Validate(DefaultSkillProfileDraft item)
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

    public void RequireValid(DefaultSkillProfileDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultSkillProfileDraft> items)
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

/// <summary>Functions of default skill profile draft rules.</summary>
public static class DefaultSkillProfileDraftRules
{
    public static DefaultSkillProfileDraftValidator DefaultDefaultSkillProfileDraftValidator()
    {
        return new DefaultSkillProfileDraftValidator(
            new List<IDefaultSkillProfileDraftRule>
            {
                new DefaultSkillProfileDraftCodeLengthRule(255),
                new DefaultSkillProfileDraftDepartmentLengthRule(120),
                new DefaultSkillProfileDraftRequiredCodeRule("default skill profile draft needs a value"),
                new DefaultSkillProfileDraftNameLengthRule(80),
            });
    }
}
