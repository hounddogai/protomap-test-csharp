namespace Acme.Workforce.Orgdesign.Headcount.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a tiered headcount claim must pass. Check returns its problems, or none.</summary>
public interface ITieredHeadcountClaimRule
{
    List<string> Check(TieredHeadcountClaim item);
}

public sealed record TieredHeadcountClaimAllowedCountryRule(List<string> Allowed) : ITieredHeadcountClaimRule
{
    public List<string> Check(TieredHeadcountClaim item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Country)))
        {
            return new List<string> { $"country {item.Country} is expired" };
        }
        return new List<string>();
    }
}

public sealed record TieredHeadcountClaimCodePrefixRule(string Prefix) : ITieredHeadcountClaimRule
{
    public List<string> Check(TieredHeadcountClaim item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TieredHeadcountClaimCodeLengthRule(int MaxLength) : ITieredHeadcountClaimRule
{
    public List<string> Check(TieredHeadcountClaim item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredHeadcountClaimRequiredLabelRule(string Message) : ITieredHeadcountClaimRule
{
    public List<string> Check(TieredHeadcountClaim item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredHeadcountClaimRequiredCodeRule(string Message) : ITieredHeadcountClaimRule
{
    public List<string> Check(TieredHeadcountClaim item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredHeadcountClaimCountryLengthRule(int MaxLength) : ITieredHeadcountClaimRule
{
    public List<string> Check(TieredHeadcountClaim item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered headcount claim.</summary>
public sealed record TieredHeadcountClaimValidator(List<ITieredHeadcountClaimRule> Rules)
{
    public List<string> Validate(TieredHeadcountClaim item)
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

    public void RequireValid(TieredHeadcountClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredHeadcountClaim> items)
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

/// <summary>Functions of tiered headcount claim rules.</summary>
public static class TieredHeadcountClaimRules
{
    public static TieredHeadcountClaimValidator DefaultTieredHeadcountClaimValidator()
    {
        return new TieredHeadcountClaimValidator(
            new List<ITieredHeadcountClaimRule>
            {
                new TieredHeadcountClaimAllowedCountryRule(new List<string> { "primary" }),
                new TieredHeadcountClaimCodePrefixRule("OPS"),
                new TieredHeadcountClaimCodeLengthRule(1000),
                new TieredHeadcountClaimRequiredLabelRule("tiered headcount claim needs an entry"),
                new TieredHeadcountClaimRequiredCodeRule("tiered headcount claim needs this field"),
                new TieredHeadcountClaimCountryLengthRule(3600),
            });
    }
}
