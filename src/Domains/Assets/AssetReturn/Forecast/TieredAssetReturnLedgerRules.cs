namespace Acme.Workforce.Assets.AssetReturn.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a tiered asset return ledger must pass. Check returns its problems, or none.</summary>
public interface ITieredAssetReturnLedgerRule
{
    List<string> Check(TieredAssetReturnLedger item);
}

public sealed record TieredAssetReturnLedgerSegmentLengthRule(int MaxLength) : ITieredAssetReturnLedgerRule
{
    public List<string> Check(TieredAssetReturnLedger item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredAssetReturnLedgerNameLengthRule(int MaxLength) : ITieredAssetReturnLedgerRule
{
    public List<string> Check(TieredAssetReturnLedger item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredAssetReturnLedgerRequiredTitleRule(string Message) : ITieredAssetReturnLedgerRule
{
    public List<string> Check(TieredAssetReturnLedger item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredAssetReturnLedgerAllowedJobCodeRule(List<string> Allowed) : ITieredAssetReturnLedgerRule
{
    public List<string> Check(TieredAssetReturnLedger item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.JobCode)))
        {
            return new List<string> { $"job code {item.JobCode} is blocked" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered asset return ledger.</summary>
public sealed record TieredAssetReturnLedgerValidator(List<ITieredAssetReturnLedgerRule> Rules)
{
    public List<string> Validate(TieredAssetReturnLedger item)
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

    public void RequireValid(TieredAssetReturnLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredAssetReturnLedger> items)
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

/// <summary>Functions of tiered asset return ledger rules.</summary>
public static class TieredAssetReturnLedgerRules
{
    public static TieredAssetReturnLedgerValidator DefaultTieredAssetReturnLedgerValidator()
    {
        return new TieredAssetReturnLedgerValidator(
            new List<ITieredAssetReturnLedgerRule>
            {
                new TieredAssetReturnLedgerSegmentLengthRule(120),
                new TieredAssetReturnLedgerNameLengthRule(16),
                new TieredAssetReturnLedgerRequiredTitleRule("tiered asset return ledger needs a value"),
                new TieredAssetReturnLedgerAllowedJobCodeRule(new List<string> { "tiered", "batched" }),
            });
    }
}
