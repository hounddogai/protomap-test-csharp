namespace Acme.Workforce.Absence.Accrual.Digest;

/// <summary>A rule a tiered accrual ledger must pass. Check returns its problems, or none.</summary>
public interface ITieredAccrualLedgerRule
{
    List<string> Check(TieredAccrualLedger item);
}

public sealed record TieredAccrualLedgerRequiredGradeRule(string Message) : ITieredAccrualLedgerRule
{
    public List<string> Check(TieredAccrualLedger item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredAccrualLedgerRequiredNameRule(string Message) : ITieredAccrualLedgerRule
{
    public List<string> Check(TieredAccrualLedger item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredAccrualLedgerGradeLengthRule(int MaxLength) : ITieredAccrualLedgerRule
{
    public List<string> Check(TieredAccrualLedger item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered accrual ledger.</summary>
public sealed record TieredAccrualLedgerValidator(List<ITieredAccrualLedgerRule> Rules)
{
    public List<string> Validate(TieredAccrualLedger item)
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

    public void RequireValid(TieredAccrualLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredAccrualLedger> items)
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

/// <summary>Functions of tiered accrual ledger rules.</summary>
public static class TieredAccrualLedgerRules
{
    public static TieredAccrualLedgerValidator DefaultTieredAccrualLedgerValidator()
    {
        return new TieredAccrualLedgerValidator(
            new List<ITieredAccrualLedgerRule>
            {
                new TieredAccrualLedgerRequiredGradeRule("tiered accrual ledger needs an entry"),
                new TieredAccrualLedgerRequiredNameRule("tiered accrual ledger needs an entry"),
                new TieredAccrualLedgerGradeLengthRule(16),
            });
    }
}
