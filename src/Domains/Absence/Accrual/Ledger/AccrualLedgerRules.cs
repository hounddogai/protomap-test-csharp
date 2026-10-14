namespace Acme.Workforce.Absence.Accrual.Ledger;

/// <summary>A rule a accrual ledger must pass. Check returns its problems, or none.</summary>
public interface IAccrualLedgerRule
{
    List<string> Check(AccrualLedger item);
}

public sealed record AccrualLedgerBatchSizeRangeRule(int Min, int Max) : IAccrualLedgerRule
{
    public List<string> Check(AccrualLedger item)
    {
        var valueValue = item.BatchSize;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"batch size {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"batch size {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AccrualLedgerMinimumRangeRule(double Min, double Max) : IAccrualLedgerRule
{
    public List<string> Check(AccrualLedger item)
    {
        var valueValue = item.Minimum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"minimum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualLedgerWindowDaysRangeRule(int Min, int Max) : IAccrualLedgerRule
{
    public List<string> Check(AccrualLedger item)
    {
        var valueValue = item.WindowDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"window days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"window days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AccrualLedgerManagerIdLengthRule(int MaxLength) : IAccrualLedgerRule
{
    public List<string> Check(AccrualLedger item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a accrual ledger.</summary>
public sealed record AccrualLedgerValidator(List<IAccrualLedgerRule> Rules)
{
    public List<string> Validate(AccrualLedger item)
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

    public void RequireValid(AccrualLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccrualLedger> items)
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

/// <summary>Functions of accrual ledger rules.</summary>
public static class AccrualLedgerRules
{
    public static AccrualLedgerValidator DefaultAccrualLedgerValidator()
    {
        return new AccrualLedgerValidator(
            new List<IAccrualLedgerRule>
            {
                new AccrualLedgerBatchSizeRangeRule(1000, 86400),
                new AccrualLedgerMinimumRangeRule(1.9, 4407.2),
                new AccrualLedgerWindowDaysRangeRule(32, 3600),
                new AccrualLedgerManagerIdLengthRule(86400),
            });
    }
}
