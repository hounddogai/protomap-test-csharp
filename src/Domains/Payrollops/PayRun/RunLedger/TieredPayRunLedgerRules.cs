namespace Acme.Workforce.Payrollops.PayRun.RunLedger;

/// <summary>A rule a tiered pay run ledger must pass. Check returns its problems, or none.</summary>
public interface ITieredPayRunLedgerRule
{
    List<string> Check(TieredPayRunLedger item);
}

public sealed record TieredPayRunLedgerRequiredNameRule(string Message) : ITieredPayRunLedgerRule
{
    public List<string> Check(TieredPayRunLedger item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredPayRunLedgerCodeLengthRule(int MaxLength) : ITieredPayRunLedgerRule
{
    public List<string> Check(TieredPayRunLedger item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredPayRunLedgerSummaryLengthRule(int MaxLength) : ITieredPayRunLedgerRule
{
    public List<string> Check(TieredPayRunLedger item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredPayRunLedgerNameLengthRule(int MaxLength) : ITieredPayRunLedgerRule
{
    public List<string> Check(TieredPayRunLedger item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredPayRunLedgerRequiredLabelRule(string Message) : ITieredPayRunLedgerRule
{
    public List<string> Check(TieredPayRunLedger item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered pay run ledger.</summary>
public sealed record TieredPayRunLedgerValidator(List<ITieredPayRunLedgerRule> Rules)
{
    public List<string> Validate(TieredPayRunLedger item)
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

    public void RequireValid(TieredPayRunLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredPayRunLedger> items)
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

/// <summary>Functions of tiered pay run ledger rules.</summary>
public static class TieredPayRunLedgerRules
{
    public static TieredPayRunLedgerValidator DefaultTieredPayRunLedgerValidator()
    {
        return new TieredPayRunLedgerValidator(
            new List<ITieredPayRunLedgerRule>
            {
                new TieredPayRunLedgerRequiredNameRule("tiered pay run ledger needs a value"),
                new TieredPayRunLedgerCodeLengthRule(32),
                new TieredPayRunLedgerSummaryLengthRule(1000),
                new TieredPayRunLedgerNameLengthRule(3600),
                new TieredPayRunLedgerRequiredLabelRule("tiered pay run ledger needs an entry"),
            });
    }
}
