namespace Acme.Workforce.Assets.AssetReturn.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a asset return agreement must pass. Check returns its problems, or none.</summary>
public interface IAssetReturnAgreementRule
{
    List<string> Check(AssetReturnAgreement item);
}

public sealed record AssetReturnAgreementAllowedTeamRule(List<string> Allowed) : IAssetReturnAgreementRule
{
    public List<string> Check(AssetReturnAgreement item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Team)))
        {
            return new List<string> { $"team {item.Team} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnAgreementCostCenterLengthRule(int MaxLength) : IAssetReturnAgreementRule
{
    public List<string> Check(AssetReturnAgreement item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnAgreementRequiredTeamRule(string Message) : IAssetReturnAgreementRule
{
    public List<string> Check(AssetReturnAgreement item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnAgreementNameLengthRule(int MaxLength) : IAssetReturnAgreementRule
{
    public List<string> Check(AssetReturnAgreement item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnAgreementCodePrefixRule(string Prefix) : IAssetReturnAgreementRule
{
    public List<string> Check(AssetReturnAgreement item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a asset return agreement.</summary>
public sealed record AssetReturnAgreementValidator(List<IAssetReturnAgreementRule> Rules)
{
    public List<string> Validate(AssetReturnAgreement item)
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

    public void RequireValid(AssetReturnAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssetReturnAgreement> items)
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

/// <summary>Functions of asset return agreement rules.</summary>
public static class AssetReturnAgreementRules
{
    public static AssetReturnAgreementValidator DefaultAssetReturnAgreementValidator()
    {
        return new AssetReturnAgreementValidator(
            new List<IAssetReturnAgreementRule>
            {
                new AssetReturnAgreementAllowedTeamRule(new List<string>()),
                new AssetReturnAgreementCostCenterLengthRule(255),
                new AssetReturnAgreementRequiredTeamRule("asset return agreement needs an entry"),
                new AssetReturnAgreementNameLengthRule(86400),
                new AssetReturnAgreementCodePrefixRule("REF"),
            });
    }
}
