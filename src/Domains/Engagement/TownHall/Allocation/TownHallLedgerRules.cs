namespace Acme.Workforce.Engagement.TownHall.Allocation;

/// <summary>A rule a town hall ledger must pass. Check returns its problems, or none.</summary>
public interface ITownHallLedgerRule
{
    List<string> Check(TownHallLedger item);
}

public sealed record TownHallLedgerRequiredCodeRule(string Message) : ITownHallLedgerRule
{
    public List<string> Check(TownHallLedger item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TownHallLedgerSummaryLengthRule(int MaxLength) : ITownHallLedgerRule
{
    public List<string> Check(TownHallLedger item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallLedgerJobCodeLengthRule(int MaxLength) : ITownHallLedgerRule
{
    public List<string> Check(TownHallLedger item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a town hall ledger.</summary>
public sealed record TownHallLedgerValidator(List<ITownHallLedgerRule> Rules)
{
    public List<string> Validate(TownHallLedger item)
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

    public void RequireValid(TownHallLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TownHallLedger> items)
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

/// <summary>Functions of town hall ledger rules.</summary>
public static class TownHallLedgerRules
{
    public static TownHallLedgerValidator DefaultTownHallLedgerValidator()
    {
        return new TownHallLedgerValidator(
            new List<ITownHallLedgerRule>
            {
                new TownHallLedgerRequiredCodeRule("town hall ledger needs a value"),
                new TownHallLedgerSummaryLengthRule(1000),
                new TownHallLedgerJobCodeLengthRule(86400),
            });
    }
}
