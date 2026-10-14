namespace Acme.Workforce.Talent.SkillProfile.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a skill profile template must pass. Check returns its problems, or none.</summary>
public interface ISkillProfileTemplateRule
{
    List<string> Check(SkillProfileTemplate item);
}

public sealed record SkillProfileTemplateAllowedReviewerRule(List<string> Allowed) : ISkillProfileTemplateRule
{
    public List<string> Check(SkillProfileTemplate item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Reviewer)))
        {
            return new List<string> { $"reviewer {item.Reviewer} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileTemplateCostRangeRule(double Min, double Max) : ISkillProfileTemplateRule
{
    public List<string> Check(SkillProfileTemplate item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileTemplateReviewerLengthRule(int MaxLength) : ISkillProfileTemplateRule
{
    public List<string> Check(SkillProfileTemplate item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a skill profile template.</summary>
public sealed record SkillProfileTemplateValidator(List<ISkillProfileTemplateRule> Rules)
{
    public List<string> Validate(SkillProfileTemplate item)
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

    public void RequireValid(SkillProfileTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SkillProfileTemplate> items)
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

/// <summary>Functions of skill profile template rules.</summary>
public static class SkillProfileTemplateRules
{
    public static SkillProfileTemplateValidator DefaultSkillProfileTemplateValidator()
    {
        return new SkillProfileTemplateValidator(
            new List<ISkillProfileTemplateRule>
            {
                new SkillProfileTemplateAllowedReviewerRule(new List<string> { "primary", "lenient", "rolling" }),
                new SkillProfileTemplateCostRangeRule(8.5, 3404.9),
                new SkillProfileTemplateReviewerLengthRule(16),
            });
    }
}
