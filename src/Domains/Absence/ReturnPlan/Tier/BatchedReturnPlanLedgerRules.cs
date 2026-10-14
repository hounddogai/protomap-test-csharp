namespace Acme.Workforce.Absence.ReturnPlan.Tier;

/// <summary>A rule a batched return plan ledger must pass. Check returns its problems, or none.</summary>
public interface IBatchedReturnPlanLedgerRule
{
    List<string> Check(BatchedReturnPlanLedger item);
}

public sealed record BatchedReturnPlanLedgerCodeLengthRule(int MaxLength) : IBatchedReturnPlanLedgerRule
{
    public List<string> Check(BatchedReturnPlanLedger item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedReturnPlanLedgerCodePrefixRule(string Prefix) : IBatchedReturnPlanLedgerRule
{
    public List<string> Check(BatchedReturnPlanLedger item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedReturnPlanLedgerRequiredCodeRule(string Message) : IBatchedReturnPlanLedgerRule
{
    public List<string> Check(BatchedReturnPlanLedger item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedReturnPlanLedgerTenureMonthsRangeRule(int Min, int Max) : IBatchedReturnPlanLedgerRule
{
    public List<string> Check(BatchedReturnPlanLedger item)
    {
        var valueValue = item.TenureMonths;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"tenure months {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedReturnPlanLedgerRequiredUnitRule(string Message) : IBatchedReturnPlanLedgerRule
{
    public List<string> Check(BatchedReturnPlanLedger item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched return plan ledger.</summary>
public sealed record BatchedReturnPlanLedgerValidator(List<IBatchedReturnPlanLedgerRule> Rules)
{
    public List<string> Validate(BatchedReturnPlanLedger item)
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

    public void RequireValid(BatchedReturnPlanLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedReturnPlanLedger> items)
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

/// <summary>Functions of batched return plan ledger rules.</summary>
public static class BatchedReturnPlanLedgerRules
{
    public static BatchedReturnPlanLedgerValidator DefaultBatchedReturnPlanLedgerValidator()
    {
        return new BatchedReturnPlanLedgerValidator(
            new List<IBatchedReturnPlanLedgerRule>
            {
                new BatchedReturnPlanLedgerCodeLengthRule(80),
                new BatchedReturnPlanLedgerCodePrefixRule("OPS"),
                new BatchedReturnPlanLedgerRequiredCodeRule("batched return plan ledger needs a value"),
                new BatchedReturnPlanLedgerTenureMonthsRangeRule(120, 1000),
                new BatchedReturnPlanLedgerRequiredUnitRule("batched return plan ledger needs this field"),
            });
    }
}
