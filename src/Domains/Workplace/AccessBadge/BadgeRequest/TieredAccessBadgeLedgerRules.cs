namespace Acme.Workforce.Workplace.AccessBadge.BadgeRequest;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a tiered access badge ledger must pass. Check returns its problems, or none.</summary>
public interface ITieredAccessBadgeLedgerRule
{
    List<string> Check(TieredAccessBadgeLedger item);
}

public sealed record TieredAccessBadgeLedgerAllowedSummaryRule(List<string> Allowed) : ITieredAccessBadgeLedgerRule
{
    public List<string> Check(TieredAccessBadgeLedger item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Summary)))
        {
            return new List<string> { $"summary {item.Summary} is stale" };
        }
        return new List<string>();
    }
}

public sealed record TieredAccessBadgeLedgerEndsAtRangeRule(int Min, int Max) : ITieredAccessBadgeLedgerRule
{
    public List<string> Check(TieredAccessBadgeLedger item)
    {
        var valueValue = item.EndsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ends at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ends at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TieredAccessBadgeLedgerRequiredBadgeRule(string Message) : ITieredAccessBadgeLedgerRule
{
    public List<string> Check(TieredAccessBadgeLedger item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered access badge ledger.</summary>
public sealed record TieredAccessBadgeLedgerValidator(List<ITieredAccessBadgeLedgerRule> Rules)
{
    public List<string> Validate(TieredAccessBadgeLedger item)
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

    public void RequireValid(TieredAccessBadgeLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredAccessBadgeLedger> items)
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

/// <summary>Functions of tiered access badge ledger rules.</summary>
public static class TieredAccessBadgeLedgerRules
{
    public static TieredAccessBadgeLedgerValidator DefaultTieredAccessBadgeLedgerValidator()
    {
        return new TieredAccessBadgeLedgerValidator(
            new List<ITieredAccessBadgeLedgerRule>
            {
                new TieredAccessBadgeLedgerAllowedSummaryRule(new List<string> { "rolling", "capped" }),
                new TieredAccessBadgeLedgerEndsAtRangeRule(80, 86400),
                new TieredAccessBadgeLedgerRequiredBadgeRule("tiered access badge ledger needs an entry"),
            });
    }
}
