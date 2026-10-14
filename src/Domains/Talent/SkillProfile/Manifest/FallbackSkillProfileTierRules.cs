namespace Acme.Workforce.Talent.SkillProfile.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a fallback skill profile tier must pass. Check returns its problems, or none.</summary>
public interface IFallbackSkillProfileTierRule
{
    List<string> Check(FallbackSkillProfileTier item);
}

public sealed record FallbackSkillProfileTierAllowedNameRule(List<string> Allowed) : IFallbackSkillProfileTierRule
{
    public List<string> Check(FallbackSkillProfileTier item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record FallbackSkillProfileTierVersionRangeRule(int Min, int Max) : IFallbackSkillProfileTierRule
{
    public List<string> Check(FallbackSkillProfileTier item)
    {
        var valueValue = item.Version;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"version {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"version {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FallbackSkillProfileTierGradeLengthRule(int MaxLength) : IFallbackSkillProfileTierRule
{
    public List<string> Check(FallbackSkillProfileTier item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackSkillProfileTierRequiredCodeRule(string Message) : IFallbackSkillProfileTierRule
{
    public List<string> Check(FallbackSkillProfileTier item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackSkillProfileTierCodePrefixRule(string Prefix) : IFallbackSkillProfileTierRule
{
    public List<string> Check(FallbackSkillProfileTier item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback skill profile tier.</summary>
public sealed record FallbackSkillProfileTierValidator(List<IFallbackSkillProfileTierRule> Rules)
{
    public List<string> Validate(FallbackSkillProfileTier item)
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

    public void RequireValid(FallbackSkillProfileTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackSkillProfileTier> items)
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

/// <summary>Functions of fallback skill profile tier rules.</summary>
public static class FallbackSkillProfileTierRules
{
    public static FallbackSkillProfileTierValidator DefaultFallbackSkillProfileTierValidator()
    {
        return new FallbackSkillProfileTierValidator(
            new List<IFallbackSkillProfileTierRule>
            {
                new FallbackSkillProfileTierAllowedNameRule(new List<string> { "global" }),
                new FallbackSkillProfileTierVersionRangeRule(32, 3600),
                new FallbackSkillProfileTierGradeLengthRule(86400),
                new FallbackSkillProfileTierRequiredCodeRule("fallback skill profile tier needs a value"),
                new FallbackSkillProfileTierCodePrefixRule("OPS"),
            });
    }
}
