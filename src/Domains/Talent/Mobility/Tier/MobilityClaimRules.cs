namespace Acme.Workforce.Talent.Mobility.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a mobility claim must pass. Check returns its problems, or none.</summary>
public interface IMobilityClaimRule
{
    List<string> Check(MobilityClaim item);
}

public sealed record MobilityClaimAllowedCategoryRule(List<string> Allowed) : IMobilityClaimRule
{
    public List<string> Check(MobilityClaim item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Category)))
        {
            return new List<string> { $"category {item.Category} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record MobilityClaimCodePrefixRule(string Prefix) : IMobilityClaimRule
{
    public List<string> Check(MobilityClaim item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record MobilityClaimNameLengthRule(int MaxLength) : IMobilityClaimRule
{
    public List<string> Check(MobilityClaim item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a mobility claim.</summary>
public sealed record MobilityClaimValidator(List<IMobilityClaimRule> Rules)
{
    public List<string> Validate(MobilityClaim item)
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

    public void RequireValid(MobilityClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MobilityClaim> items)
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

/// <summary>Functions of mobility claim rules.</summary>
public static class MobilityClaimRules
{
    public static MobilityClaimValidator DefaultMobilityClaimValidator()
    {
        return new MobilityClaimValidator(
            new List<IMobilityClaimRule>
            {
                new MobilityClaimAllowedCategoryRule(new List<string> { "seasonal", "default", "adaptive" }),
                new MobilityClaimCodePrefixRule("OPS"),
                new MobilityClaimNameLengthRule(86400),
            });
    }
}
