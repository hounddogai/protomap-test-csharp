namespace Acme.Workforce.Payrollops.PayRun.CappedRunLedger;

/// <summary>A rule a default pay run ledger must pass. Check returns its problems, or none.</summary>
public interface IDefaultPayRunLedgerRule
{
    List<string> Check(DefaultPayRunLedger item);
}

public sealed record DefaultPayRunLedgerGraceDaysRangeRule(int Min, int Max) : IDefaultPayRunLedgerRule
{
    public List<string> Check(DefaultPayRunLedger item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultPayRunLedgerWindowDaysRangeRule(int Min, int Max) : IDefaultPayRunLedgerRule
{
    public List<string> Check(DefaultPayRunLedger item)
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

public sealed record DefaultPayRunLedgerSourceLengthRule(int MaxLength) : IDefaultPayRunLedgerRule
{
    public List<string> Check(DefaultPayRunLedger item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default pay run ledger.</summary>
public sealed record DefaultPayRunLedgerValidator(List<IDefaultPayRunLedgerRule> Rules)
{
    public List<string> Validate(DefaultPayRunLedger item)
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

    public void RequireValid(DefaultPayRunLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultPayRunLedger> items)
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

/// <summary>Functions of default pay run ledger rules.</summary>
public static class DefaultPayRunLedgerRules
{
    public static DefaultPayRunLedgerValidator DefaultDefaultPayRunLedgerValidator()
    {
        return new DefaultPayRunLedgerValidator(
            new List<IDefaultPayRunLedgerRule>
            {
                new DefaultPayRunLedgerGraceDaysRangeRule(16, 86400),
                new DefaultPayRunLedgerWindowDaysRangeRule(64, 86400),
                new DefaultPayRunLedgerSourceLengthRule(16),
            });
    }
}
