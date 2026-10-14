namespace Acme.Workforce.Talent.SkillProfile.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scheduled skill profile policy must pass. Check returns its problems, or none.</summary>
public interface IScheduledSkillProfilePolicyRule
{
    List<string> Check(ScheduledSkillProfilePolicy item);
}

public sealed record ScheduledSkillProfilePolicyAllowedLabelRule(List<string> Allowed) : IScheduledSkillProfilePolicyRule
{
    public List<string> Check(ScheduledSkillProfilePolicy item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is missing" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledSkillProfilePolicyRequiredCodeRule(string Message) : IScheduledSkillProfilePolicyRule
{
    public List<string> Check(ScheduledSkillProfilePolicy item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledSkillProfilePolicyJobCodeLengthRule(int MaxLength) : IScheduledSkillProfilePolicyRule
{
    public List<string> Check(ScheduledSkillProfilePolicy item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledSkillProfilePolicyLabelLengthRule(int MaxLength) : IScheduledSkillProfilePolicyRule
{
    public List<string> Check(ScheduledSkillProfilePolicy item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledSkillProfilePolicyRequiredJobCodeRule(string Message) : IScheduledSkillProfilePolicyRule
{
    public List<string> Check(ScheduledSkillProfilePolicy item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled skill profile policy.</summary>
public sealed record ScheduledSkillProfilePolicyValidator(List<IScheduledSkillProfilePolicyRule> Rules)
{
    public List<string> Validate(ScheduledSkillProfilePolicy item)
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

    public void RequireValid(ScheduledSkillProfilePolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledSkillProfilePolicy> items)
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

/// <summary>Functions of scheduled skill profile policy rules.</summary>
public static class ScheduledSkillProfilePolicyRules
{
    public static ScheduledSkillProfilePolicyValidator DefaultScheduledSkillProfilePolicyValidator()
    {
        return new ScheduledSkillProfilePolicyValidator(
            new List<IScheduledSkillProfilePolicyRule>
            {
                new ScheduledSkillProfilePolicyAllowedLabelRule(new List<string> { "lenient" }),
                new ScheduledSkillProfilePolicyRequiredCodeRule("scheduled skill profile policy needs a value"),
                new ScheduledSkillProfilePolicyJobCodeLengthRule(64),
                new ScheduledSkillProfilePolicyLabelLengthRule(86400),
                new ScheduledSkillProfilePolicyRequiredJobCodeRule("scheduled skill profile policy needs a value"),
            });
    }
}
