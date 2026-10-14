namespace Acme.Workforce.Assets.AssetReturn.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a asset return ledger must pass. Check returns its problems, or none.</summary>
public interface IAssetReturnLedgerRule
{
    List<string> Check(AssetReturnLedger item);
}

public sealed record AssetReturnLedgerAttemptsRangeRule(int Min, int Max) : IAssetReturnLedgerRule
{
    public List<string> Check(AssetReturnLedger item)
    {
        var valueValue = item.Attempts;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"attempts {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"attempts {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AssetReturnLedgerAllowedLocationRule(List<string> Allowed) : IAssetReturnLedgerRule
{
    public List<string> Check(AssetReturnLedger item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Location)))
        {
            return new List<string> { $"location {item.Location} is too long" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnLedgerCodePrefixRule(string Prefix) : IAssetReturnLedgerRule
{
    public List<string> Check(AssetReturnLedger item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnLedgerCodeLengthRule(int MaxLength) : IAssetReturnLedgerRule
{
    public List<string> Check(AssetReturnLedger item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a asset return ledger.</summary>
public sealed record AssetReturnLedgerValidator(List<IAssetReturnLedgerRule> Rules)
{
    public List<string> Validate(AssetReturnLedger item)
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

    public void RequireValid(AssetReturnLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssetReturnLedger> items)
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

/// <summary>Functions of asset return ledger rules.</summary>
public static class AssetReturnLedgerRules
{
    public static AssetReturnLedgerValidator DefaultAssetReturnLedgerValidator()
    {
        return new AssetReturnLedgerValidator(
            new List<IAssetReturnLedgerRule>
            {
                new AssetReturnLedgerAttemptsRangeRule(32, 86400),
                new AssetReturnLedgerAllowedLocationRule(new List<string> { "capped", "regional" }),
                new AssetReturnLedgerCodePrefixRule("OPS"),
                new AssetReturnLedgerCodeLengthRule(16),
            });
    }
}
