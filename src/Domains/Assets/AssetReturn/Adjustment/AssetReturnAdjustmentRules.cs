namespace Acme.Workforce.Assets.AssetReturn.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a asset return adjustment must pass. Check returns its problems, or none.</summary>
public interface IAssetReturnAdjustmentRule
{
    List<string> Check(AssetReturnAdjustment item);
}

public sealed record AssetReturnAdjustmentChecksumLengthRule(int MaxLength) : IAssetReturnAdjustmentRule
{
    public List<string> Check(AssetReturnAdjustment item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnAdjustmentUnitLengthRule(int MaxLength) : IAssetReturnAdjustmentRule
{
    public List<string> Check(AssetReturnAdjustment item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnAdjustmentCodePrefixRule(string Prefix) : IAssetReturnAdjustmentRule
{
    public List<string> Check(AssetReturnAdjustment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnAdjustmentRequiredChecksumRule(string Message) : IAssetReturnAdjustmentRule
{
    public List<string> Check(AssetReturnAdjustment item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnAdjustmentAllowedChecksumRule(List<string> Allowed) : IAssetReturnAdjustmentRule
{
    public List<string> Check(AssetReturnAdjustment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Checksum)))
        {
            return new List<string> { $"checksum {item.Checksum} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnAdjustmentRequiredTitleRule(string Message) : IAssetReturnAdjustmentRule
{
    public List<string> Check(AssetReturnAdjustment item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a asset return adjustment.</summary>
public sealed record AssetReturnAdjustmentValidator(List<IAssetReturnAdjustmentRule> Rules)
{
    public List<string> Validate(AssetReturnAdjustment item)
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

    public void RequireValid(AssetReturnAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssetReturnAdjustment> items)
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

/// <summary>Functions of asset return adjustment rules.</summary>
public static class AssetReturnAdjustmentRules
{
    public static AssetReturnAdjustmentValidator DefaultAssetReturnAdjustmentValidator()
    {
        return new AssetReturnAdjustmentValidator(
            new List<IAssetReturnAdjustmentRule>
            {
                new AssetReturnAdjustmentChecksumLengthRule(80),
                new AssetReturnAdjustmentUnitLengthRule(80),
                new AssetReturnAdjustmentCodePrefixRule("ACM"),
                new AssetReturnAdjustmentRequiredChecksumRule("asset return adjustment needs a value"),
                new AssetReturnAdjustmentAllowedChecksumRule(new List<string> { "pending" }),
                new AssetReturnAdjustmentRequiredTitleRule("asset return adjustment needs this field"),
            });
    }
}
