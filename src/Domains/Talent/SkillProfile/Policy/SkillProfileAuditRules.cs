namespace Acme.Workforce.Talent.SkillProfile.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a skill profile audit must pass. Check returns its problems, or none.</summary>
public interface ISkillProfileAuditRule
{
    List<string> Check(SkillProfileAudit item);
}

public sealed record SkillProfileAuditTitleLengthRule(int MaxLength) : ISkillProfileAuditRule
{
    public List<string> Check(SkillProfileAudit item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileAuditAllowedExternalRefRule(List<string> Allowed) : ISkillProfileAuditRule
{
    public List<string> Check(SkillProfileAudit item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ExternalRef)))
        {
            return new List<string> { $"external ref {item.ExternalRef} is too long" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileAuditRequiredCodeRule(string Message) : ISkillProfileAuditRule
{
    public List<string> Check(SkillProfileAudit item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a skill profile audit.</summary>
public sealed record SkillProfileAuditValidator(List<ISkillProfileAuditRule> Rules)
{
    public List<string> Validate(SkillProfileAudit item)
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

    public void RequireValid(SkillProfileAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SkillProfileAudit> items)
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

/// <summary>Functions of skill profile audit rules.</summary>
public static class SkillProfileAuditRules
{
    public static SkillProfileAuditValidator DefaultSkillProfileAuditValidator()
    {
        return new SkillProfileAuditValidator(
            new List<ISkillProfileAuditRule>
            {
                new SkillProfileAuditTitleLengthRule(3600),
                new SkillProfileAuditAllowedExternalRefRule(new List<string> { "default", "primary", "adaptive" }),
                new SkillProfileAuditRequiredCodeRule("skill profile audit needs an entry"),
            });
    }
}
