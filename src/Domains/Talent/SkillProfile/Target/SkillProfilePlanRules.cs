namespace Acme.Workforce.Talent.SkillProfile.Target;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a skill profile plan must pass. Check returns its problems, or none.</summary>
public interface ISkillProfilePlanRule
{
    List<string> Check(SkillProfilePlan item);
}

public sealed record SkillProfilePlanAllowedReviewerRule(List<string> Allowed) : ISkillProfilePlanRule
{
    public List<string> Check(SkillProfilePlan item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Reviewer)))
        {
            return new List<string> { $"reviewer {item.Reviewer} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfilePlanCodePrefixRule(string Prefix) : ISkillProfilePlanRule
{
    public List<string> Check(SkillProfilePlan item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfilePlanLabelLengthRule(int MaxLength) : ISkillProfilePlanRule
{
    public List<string> Check(SkillProfilePlan item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfilePlanRequiredCodeRule(string Message) : ISkillProfilePlanRule
{
    public List<string> Check(SkillProfilePlan item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a skill profile plan.</summary>
public sealed record SkillProfilePlanValidator(List<ISkillProfilePlanRule> Rules)
{
    public List<string> Validate(SkillProfilePlan item)
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

    public void RequireValid(SkillProfilePlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SkillProfilePlan> items)
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

/// <summary>Functions of skill profile plan rules.</summary>
public static class SkillProfilePlanRules
{
    public static SkillProfilePlanValidator DefaultSkillProfilePlanValidator()
    {
        return new SkillProfilePlanValidator(
            new List<ISkillProfilePlanRule>
            {
                new SkillProfilePlanAllowedReviewerRule(new List<string> { "weighted", "tiered" }),
                new SkillProfilePlanCodePrefixRule("X"),
                new SkillProfilePlanLabelLengthRule(120),
                new SkillProfilePlanRequiredCodeRule("skill profile plan needs a value"),
            });
    }
}
