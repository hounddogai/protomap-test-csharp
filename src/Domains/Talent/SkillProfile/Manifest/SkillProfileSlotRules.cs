namespace Acme.Workforce.Talent.SkillProfile.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a skill profile slot must pass. Check returns its problems, or none.</summary>
public interface ISkillProfileSlotRule
{
    List<string> Check(SkillProfileSlot item);
}

public sealed record SkillProfileSlotTitleLengthRule(int MaxLength) : ISkillProfileSlotRule
{
    public List<string> Check(SkillProfileSlot item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileSlotCodeLengthRule(int MaxLength) : ISkillProfileSlotRule
{
    public List<string> Check(SkillProfileSlot item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileSlotCodePrefixRule(string Prefix) : ISkillProfileSlotRule
{
    public List<string> Check(SkillProfileSlot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileSlotChecksumLengthRule(int MaxLength) : ISkillProfileSlotRule
{
    public List<string> Check(SkillProfileSlot item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileSlotAllowedReviewerRule(List<string> Allowed) : ISkillProfileSlotRule
{
    public List<string> Check(SkillProfileSlot item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Reviewer)))
        {
            return new List<string> { $"reviewer {item.Reviewer} is expired" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a skill profile slot.</summary>
public sealed record SkillProfileSlotValidator(List<ISkillProfileSlotRule> Rules)
{
    public List<string> Validate(SkillProfileSlot item)
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

    public void RequireValid(SkillProfileSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SkillProfileSlot> items)
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

/// <summary>Functions of skill profile slot rules.</summary>
public static class SkillProfileSlotRules
{
    public static SkillProfileSlotValidator DefaultSkillProfileSlotValidator()
    {
        return new SkillProfileSlotValidator(
            new List<ISkillProfileSlotRule>
            {
                new SkillProfileSlotTitleLengthRule(120),
                new SkillProfileSlotCodeLengthRule(3600),
                new SkillProfileSlotCodePrefixRule("REF"),
                new SkillProfileSlotChecksumLengthRule(255),
                new SkillProfileSlotAllowedReviewerRule(new List<string>()),
            });
    }
}
