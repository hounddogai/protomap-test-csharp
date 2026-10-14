namespace Acme.Workforce.Talent.SkillProfile.Bundle;

/// <summary>A rule a skill profile bundle must pass. Check returns its problems, or none.</summary>
public interface ISkillProfileBundleRule
{
    List<string> Check(SkillProfileBundle item);
}

public sealed record SkillProfileBundleLabelLengthRule(int MaxLength) : ISkillProfileBundleRule
{
    public List<string> Check(SkillProfileBundle item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileBundleRequiredExternalRefRule(string Message) : ISkillProfileBundleRule
{
    public List<string> Check(SkillProfileBundle item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileBundleRequiredLabelRule(string Message) : ISkillProfileBundleRule
{
    public List<string> Check(SkillProfileBundle item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a skill profile bundle.</summary>
public sealed record SkillProfileBundleValidator(List<ISkillProfileBundleRule> Rules)
{
    public List<string> Validate(SkillProfileBundle item)
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

    public void RequireValid(SkillProfileBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SkillProfileBundle> items)
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

/// <summary>Functions of skill profile bundle rules.</summary>
public static class SkillProfileBundleRules
{
    public static SkillProfileBundleValidator DefaultSkillProfileBundleValidator()
    {
        return new SkillProfileBundleValidator(
            new List<ISkillProfileBundleRule>
            {
                new SkillProfileBundleLabelLengthRule(32),
                new SkillProfileBundleRequiredExternalRefRule("skill profile bundle needs this field"),
                new SkillProfileBundleRequiredLabelRule("skill profile bundle needs an entry"),
            });
    }
}
