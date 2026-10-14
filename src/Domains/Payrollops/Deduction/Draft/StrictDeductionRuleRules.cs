namespace Acme.Workforce.Payrollops.Deduction.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a strict deduction rule must pass. Check returns its problems, or none.</summary>
public interface IStrictDeductionRuleRule
{
    List<string> Check(StrictDeductionRule item);
}

public sealed record StrictDeductionRuleCodePrefixRule(string Prefix) : IStrictDeductionRuleRule
{
    public List<string> Check(StrictDeductionRule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record StrictDeductionRuleLabelLengthRule(int MaxLength) : IStrictDeductionRuleRule
{
    public List<string> Check(StrictDeductionRule item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictDeductionRuleSkillLengthRule(int MaxLength) : IStrictDeductionRuleRule
{
    public List<string> Check(StrictDeductionRule item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictDeductionRuleAllowedSkillRule(List<string> Allowed) : IStrictDeductionRuleRule
{
    public List<string> Check(StrictDeductionRule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Skill)))
        {
            return new List<string> { $"skill {item.Skill} is not allowed" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict deduction rule.</summary>
public sealed record StrictDeductionRuleValidator(List<IStrictDeductionRuleRule> Rules)
{
    public List<string> Validate(StrictDeductionRule item)
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

    public void RequireValid(StrictDeductionRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictDeductionRule> items)
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

/// <summary>Functions of strict deduction rule rules.</summary>
public static class StrictDeductionRuleRules
{
    public static StrictDeductionRuleValidator DefaultStrictDeductionRuleValidator()
    {
        return new StrictDeductionRuleValidator(
            new List<IStrictDeductionRuleRule>
            {
                new StrictDeductionRuleCodePrefixRule("OPS"),
                new StrictDeductionRuleLabelLengthRule(32),
                new StrictDeductionRuleSkillLengthRule(1000),
                new StrictDeductionRuleAllowedSkillRule(new List<string> { "adaptive" }),
            });
    }
}
