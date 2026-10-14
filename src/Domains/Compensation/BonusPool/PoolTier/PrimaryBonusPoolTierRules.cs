namespace Acme.Workforce.Compensation.BonusPool.PoolTier;

/// <summary>A rule a primary bonus pool tier must pass. Check returns its problems, or none.</summary>
public interface IPrimaryBonusPoolTierRule
{
    List<string> Check(PrimaryBonusPoolTier item);
}

public sealed record PrimaryBonusPoolTierRequiredSalaryBandRule(string Message) : IPrimaryBonusPoolTierRule
{
    public List<string> Check(PrimaryBonusPoolTier item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryBonusPoolTierRequiredCostCenterRule(string Message) : IPrimaryBonusPoolTierRule
{
    public List<string> Check(PrimaryBonusPoolTier item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryBonusPoolTierCodeLengthRule(int MaxLength) : IPrimaryBonusPoolTierRule
{
    public List<string> Check(PrimaryBonusPoolTier item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryBonusPoolTierRequiredCodeRule(string Message) : IPrimaryBonusPoolTierRule
{
    public List<string> Check(PrimaryBonusPoolTier item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryBonusPoolTierCodePrefixRule(string Prefix) : IPrimaryBonusPoolTierRule
{
    public List<string> Check(PrimaryBonusPoolTier item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary bonus pool tier.</summary>
public sealed record PrimaryBonusPoolTierValidator(List<IPrimaryBonusPoolTierRule> Rules)
{
    public List<string> Validate(PrimaryBonusPoolTier item)
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

    public void RequireValid(PrimaryBonusPoolTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryBonusPoolTier> items)
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

/// <summary>Functions of primary bonus pool tier rules.</summary>
public static class PrimaryBonusPoolTierRules
{
    public static PrimaryBonusPoolTierValidator DefaultPrimaryBonusPoolTierValidator()
    {
        return new PrimaryBonusPoolTierValidator(
            new List<IPrimaryBonusPoolTierRule>
            {
                new PrimaryBonusPoolTierRequiredSalaryBandRule("primary bonus pool tier needs this field"),
                new PrimaryBonusPoolTierRequiredCostCenterRule("primary bonus pool tier needs a value"),
                new PrimaryBonusPoolTierCodeLengthRule(120),
                new PrimaryBonusPoolTierRequiredCodeRule("primary bonus pool tier needs a value"),
                new PrimaryBonusPoolTierCodePrefixRule("REF"),
            });
    }
}
