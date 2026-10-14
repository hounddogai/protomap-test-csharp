namespace Acme.Workforce.Payrollops.Garnishment.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a garnishment tier must pass. Check returns its problems, or none.</summary>
public interface IGarnishmentTierRule
{
    List<string> Check(GarnishmentTier item);
}

public sealed record GarnishmentTierCodeLengthRule(int MaxLength) : IGarnishmentTierRule
{
    public List<string> Check(GarnishmentTier item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentTierCodePrefixRule(string Prefix) : IGarnishmentTierRule
{
    public List<string> Check(GarnishmentTier item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentTierAllowedRegionRule(List<string> Allowed) : IGarnishmentTierRule
{
    public List<string> Check(GarnishmentTier item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Region)))
        {
            return new List<string> { $"region {item.Region} is missing" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentTierEmployeeIdLengthRule(int MaxLength) : IGarnishmentTierRule
{
    public List<string> Check(GarnishmentTier item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentTierPinnedGuardRule(int Limit) : IGarnishmentTierRule
{
    public List<string> Check(GarnishmentTier item)
    {
        if (item.Pinned && item.Attempts > Limit)
        {
            return new List<string> { $"attempts {item.Attempts} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a garnishment tier.</summary>
public sealed record GarnishmentTierValidator(List<IGarnishmentTierRule> Rules)
{
    public List<string> Validate(GarnishmentTier item)
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

    public void RequireValid(GarnishmentTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GarnishmentTier> items)
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

/// <summary>Functions of garnishment tier rules.</summary>
public static class GarnishmentTierRules
{
    public static GarnishmentTierValidator DefaultGarnishmentTierValidator()
    {
        return new GarnishmentTierValidator(
            new List<IGarnishmentTierRule>
            {
                new GarnishmentTierCodeLengthRule(86400),
                new GarnishmentTierCodePrefixRule("X"),
                new GarnishmentTierAllowedRegionRule(new List<string>()),
                new GarnishmentTierEmployeeIdLengthRule(1000),
                new GarnishmentTierPinnedGuardRule(255),
            });
    }
}
