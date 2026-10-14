namespace Acme.Workforce.Talent.SkillProfile.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a skill profile adjustment must pass. Check returns its problems, or none.</summary>
public interface ISkillProfileAdjustmentRule
{
    List<string> Check(SkillProfileAdjustment item);
}

public sealed record SkillProfileAdjustmentWindowDaysRangeRule(int Min, int Max) : ISkillProfileAdjustmentRule
{
    public List<string> Check(SkillProfileAdjustment item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileAdjustmentCodePrefixRule(string Prefix) : ISkillProfileAdjustmentRule
{
    public List<string> Check(SkillProfileAdjustment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileAdjustmentAllowedChecksumRule(List<string> Allowed) : ISkillProfileAdjustmentRule
{
    public List<string> Check(SkillProfileAdjustment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Checksum)))
        {
            return new List<string> { $"checksum {item.Checksum} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileAdjustmentCodeLengthRule(int MaxLength) : ISkillProfileAdjustmentRule
{
    public List<string> Check(SkillProfileAdjustment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a skill profile adjustment.</summary>
public sealed record SkillProfileAdjustmentValidator(List<ISkillProfileAdjustmentRule> Rules)
{
    public List<string> Validate(SkillProfileAdjustment item)
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

    public void RequireValid(SkillProfileAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SkillProfileAdjustment> items)
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

/// <summary>Functions of skill profile adjustment rules.</summary>
public static class SkillProfileAdjustmentRules
{
    public static SkillProfileAdjustmentValidator DefaultSkillProfileAdjustmentValidator()
    {
        return new SkillProfileAdjustmentValidator(
            new List<ISkillProfileAdjustmentRule>
            {
                new SkillProfileAdjustmentWindowDaysRangeRule(16, 86400),
                new SkillProfileAdjustmentCodePrefixRule("X"),
                new SkillProfileAdjustmentAllowedChecksumRule(new List<string> { "strict", "regional" }),
                new SkillProfileAdjustmentCodeLengthRule(86400),
            });
    }
}
