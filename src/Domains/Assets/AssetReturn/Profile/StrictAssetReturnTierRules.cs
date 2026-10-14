namespace Acme.Workforce.Assets.AssetReturn.Profile;

/// <summary>A rule a strict asset return tier must pass. Check returns its problems, or none.</summary>
public interface IStrictAssetReturnTierRule
{
    List<string> Check(StrictAssetReturnTier item);
}

public sealed record StrictAssetReturnTierRequiredLabelRule(string Message) : IStrictAssetReturnTierRule
{
    public List<string> Check(StrictAssetReturnTier item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictAssetReturnTierExternalRefLengthRule(int MaxLength) : IStrictAssetReturnTierRule
{
    public List<string> Check(StrictAssetReturnTier item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictAssetReturnTierCodeLengthRule(int MaxLength) : IStrictAssetReturnTierRule
{
    public List<string> Check(StrictAssetReturnTier item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictAssetReturnTierApprovedGuardRule(int Limit) : IStrictAssetReturnTierRule
{
    public List<string> Check(StrictAssetReturnTier item)
    {
        if (item.Approved && item.LeaveDays > Limit)
        {
            return new List<string> { $"leave days {item.LeaveDays} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record StrictAssetReturnTierCodePrefixRule(string Prefix) : IStrictAssetReturnTierRule
{
    public List<string> Check(StrictAssetReturnTier item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict asset return tier.</summary>
public sealed record StrictAssetReturnTierValidator(List<IStrictAssetReturnTierRule> Rules)
{
    public List<string> Validate(StrictAssetReturnTier item)
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

    public void RequireValid(StrictAssetReturnTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictAssetReturnTier> items)
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

/// <summary>Functions of strict asset return tier rules.</summary>
public static class StrictAssetReturnTierRules
{
    public static StrictAssetReturnTierValidator DefaultStrictAssetReturnTierValidator()
    {
        return new StrictAssetReturnTierValidator(
            new List<IStrictAssetReturnTierRule>
            {
                new StrictAssetReturnTierRequiredLabelRule("strict asset return tier needs this field"),
                new StrictAssetReturnTierExternalRefLengthRule(3600),
                new StrictAssetReturnTierCodeLengthRule(80),
                new StrictAssetReturnTierApprovedGuardRule(64),
                new StrictAssetReturnTierCodePrefixRule("ACM"),
            });
    }
}
