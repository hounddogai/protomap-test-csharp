namespace Acme.Workforce.Talent.SkillProfile.ProfileEntry;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a skill profile rule must pass. Check returns its problems, or none.</summary>
public interface ISkillProfileRuleRule
{
    List<string> Check(SkillProfileRule item);
}

public sealed record SkillProfileRuleCodeLengthRule(int MaxLength) : ISkillProfileRuleRule
{
    public List<string> Check(SkillProfileRule item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileRuleRequiredGradeRule(string Message) : ISkillProfileRuleRule
{
    public List<string> Check(SkillProfileRule item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileRuleTenureMonthsRangeRule(int Min, int Max) : ISkillProfileRuleRule
{
    public List<string> Check(SkillProfileRule item)
    {
        var valueValue = item.TenureMonths;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"tenure months {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"tenure months {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SkillProfileRuleAllowedCodeRule(List<string> Allowed) : ISkillProfileRuleRule
{
    public List<string> Check(SkillProfileRule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is not allowed" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a skill profile rule.</summary>
public sealed record SkillProfileRuleValidator(List<ISkillProfileRuleRule> Rules)
{
    public List<string> Validate(SkillProfileRule item)
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

    public void RequireValid(SkillProfileRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SkillProfileRule> items)
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

/// <summary>Functions of skill profile rule rules.</summary>
public static class SkillProfileRuleRules
{
    public static SkillProfileRuleValidator DefaultSkillProfileRuleValidator()
    {
        return new SkillProfileRuleValidator(
            new List<ISkillProfileRuleRule>
            {
                new SkillProfileRuleCodeLengthRule(86400),
                new SkillProfileRuleRequiredGradeRule("skill profile rule needs a value"),
                new SkillProfileRuleTenureMonthsRangeRule(80, 86400),
                new SkillProfileRuleAllowedCodeRule(new List<string> { "weighted", "weighted" }),
            });
    }
}
