namespace Acme.Workforce.Talent.SkillProfile.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a skill profile mapping must pass. Check returns its problems, or none.</summary>
public interface ISkillProfileMappingRule
{
    List<string> Check(SkillProfileMapping item);
}

public sealed record SkillProfileMappingCodePrefixRule(string Prefix) : ISkillProfileMappingRule
{
    public List<string> Check(SkillProfileMapping item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileMappingTitleLengthRule(int MaxLength) : ISkillProfileMappingRule
{
    public List<string> Check(SkillProfileMapping item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileMappingBadgeLengthRule(int MaxLength) : ISkillProfileMappingRule
{
    public List<string> Check(SkillProfileMapping item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileMappingAllowedNotesRule(List<string> Allowed) : ISkillProfileMappingRule
{
    public List<string> Check(SkillProfileMapping item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Notes)))
        {
            return new List<string> { $"notes {item.Notes} is inconsistent" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a skill profile mapping.</summary>
public sealed record SkillProfileMappingValidator(List<ISkillProfileMappingRule> Rules)
{
    public List<string> Validate(SkillProfileMapping item)
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

    public void RequireValid(SkillProfileMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SkillProfileMapping> items)
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

/// <summary>Functions of skill profile mapping rules.</summary>
public static class SkillProfileMappingRules
{
    public static SkillProfileMappingValidator DefaultSkillProfileMappingValidator()
    {
        return new SkillProfileMappingValidator(
            new List<ISkillProfileMappingRule>
            {
                new SkillProfileMappingCodePrefixRule("OPS"),
                new SkillProfileMappingTitleLengthRule(3600),
                new SkillProfileMappingBadgeLengthRule(1000),
                new SkillProfileMappingAllowedNotesRule(new List<string> { "batched", "rolling" }),
            });
    }
}
