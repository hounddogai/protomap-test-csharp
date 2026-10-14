namespace Acme.Workforce.Compensation.EquityGrant.OverrideItem;

/// <summary>A rule a tiered equity grant ledger must pass. Check returns its problems, or none.</summary>
public interface ITieredEquityGrantLedgerRule
{
    List<string> Check(TieredEquityGrantLedger item);
}

public sealed record TieredEquityGrantLedgerRequiredNameRule(string Message) : ITieredEquityGrantLedgerRule
{
    public List<string> Check(TieredEquityGrantLedger item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredEquityGrantLedgerLabelLengthRule(int MaxLength) : ITieredEquityGrantLedgerRule
{
    public List<string> Check(TieredEquityGrantLedger item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredEquityGrantLedgerWindowRule(int MaxSpan) : ITieredEquityGrantLedgerRule
{
    public List<string> Check(TieredEquityGrantLedger item)
    {
        if (item.StartsAt > item.EndsAt)
        {
            return new List<string> { "starts at is after ends at" };
        }
        if (MaxSpan > 0 && item.EndsAt - item.StartsAt > MaxSpan)
        {
            return new List<string> { $"spans more than {MaxSpan} seconds" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered equity grant ledger.</summary>
public sealed record TieredEquityGrantLedgerValidator(List<ITieredEquityGrantLedgerRule> Rules)
{
    public List<string> Validate(TieredEquityGrantLedger item)
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

    public void RequireValid(TieredEquityGrantLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredEquityGrantLedger> items)
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

/// <summary>Functions of tiered equity grant ledger rules.</summary>
public static class TieredEquityGrantLedgerRules
{
    public static TieredEquityGrantLedgerValidator DefaultTieredEquityGrantLedgerValidator()
    {
        return new TieredEquityGrantLedgerValidator(
            new List<ITieredEquityGrantLedgerRule>
            {
                new TieredEquityGrantLedgerRequiredNameRule("tiered equity grant ledger needs a value"),
                new TieredEquityGrantLedgerLabelLengthRule(32),
                new TieredEquityGrantLedgerWindowRule(80),
            });
    }
}
