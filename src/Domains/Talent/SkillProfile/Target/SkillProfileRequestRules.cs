namespace Acme.Workforce.Talent.SkillProfile.Target;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a skill profile request must pass. Check returns its problems, or none.</summary>
public interface ISkillProfileRequestRule
{
    List<string> Check(SkillProfileRequest item);
}

public sealed record SkillProfileRequestAllowedBadgeRule(List<string> Allowed) : ISkillProfileRequestRule
{
    public List<string> Check(SkillProfileRequest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Badge)))
        {
            return new List<string> { $"badge {item.Badge} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileRequestExpiresAtRangeRule(int Min, int Max) : ISkillProfileRequestRule
{
    public List<string> Check(SkillProfileRequest item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileRequestCodeLengthRule(int MaxLength) : ISkillProfileRequestRule
{
    public List<string> Check(SkillProfileRequest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileRequestCodePrefixRule(string Prefix) : ISkillProfileRequestRule
{
    public List<string> Check(SkillProfileRequest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a skill profile request.</summary>
public sealed record SkillProfileRequestValidator(List<ISkillProfileRequestRule> Rules)
{
    public List<string> Validate(SkillProfileRequest item)
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

    public void RequireValid(SkillProfileRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SkillProfileRequest> items)
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

/// <summary>Functions of skill profile request rules.</summary>
public static class SkillProfileRequestRules
{
    public static SkillProfileRequestValidator DefaultSkillProfileRequestValidator()
    {
        return new SkillProfileRequestValidator(
            new List<ISkillProfileRequestRule>
            {
                new SkillProfileRequestAllowedBadgeRule(new List<string> { "weighted", "adaptive" }),
                new SkillProfileRequestExpiresAtRangeRule(32, 255),
                new SkillProfileRequestCodeLengthRule(255),
                new SkillProfileRequestCodePrefixRule("ACM"),
            });
    }
}
