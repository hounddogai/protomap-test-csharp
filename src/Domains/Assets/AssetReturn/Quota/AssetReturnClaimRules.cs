namespace Acme.Workforce.Assets.AssetReturn.Quota;

/// <summary>A rule a asset return claim must pass. Check returns its problems, or none.</summary>
public interface IAssetReturnClaimRule
{
    List<string> Check(AssetReturnClaim item);
}

public sealed record AssetReturnClaimCodeLengthRule(int MaxLength) : IAssetReturnClaimRule
{
    public List<string> Check(AssetReturnClaim item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnClaimRequiredLabelRule(string Message) : IAssetReturnClaimRule
{
    public List<string> Check(AssetReturnClaim item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnClaimPhoneLengthRule(int MaxLength) : IAssetReturnClaimRule
{
    public List<string> Check(AssetReturnClaim item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnClaimRequiredPhoneRule(string Message) : IAssetReturnClaimRule
{
    public List<string> Check(AssetReturnClaim item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a asset return claim.</summary>
public sealed record AssetReturnClaimValidator(List<IAssetReturnClaimRule> Rules)
{
    public List<string> Validate(AssetReturnClaim item)
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

    public void RequireValid(AssetReturnClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssetReturnClaim> items)
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

/// <summary>Functions of asset return claim rules.</summary>
public static class AssetReturnClaimRules
{
    public static AssetReturnClaimValidator DefaultAssetReturnClaimValidator()
    {
        return new AssetReturnClaimValidator(
            new List<IAssetReturnClaimRule>
            {
                new AssetReturnClaimCodeLengthRule(16),
                new AssetReturnClaimRequiredLabelRule("asset return claim needs a value"),
                new AssetReturnClaimPhoneLengthRule(86400),
                new AssetReturnClaimRequiredPhoneRule("asset return claim needs this field"),
            });
    }
}
