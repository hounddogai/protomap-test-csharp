namespace Acme.Workforce.Orgdesign.Reorg.Summary;

/// <summary>A rule a reorg ledger must pass. Check returns its problems, or none.</summary>
public interface IReorgLedgerRule
{
    List<string> Check(ReorgLedger item);
}

public sealed record ReorgLedgerScoreRangeRule(double Min, double Max) : IReorgLedgerRule
{
    public List<string> Check(ReorgLedger item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ReorgLedgerApprovedGuardRule(double Limit) : IReorgLedgerRule
{
    public List<string> Check(ReorgLedger item)
    {
        if (item.Approved && item.Score > Limit)
        {
            return new List<string> { $"score {item.Score} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record ReorgLedgerRequiredNameRule(string Message) : IReorgLedgerRule
{
    public List<string> Check(ReorgLedger item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReorgLedgerRequiredTitleRule(string Message) : IReorgLedgerRule
{
    public List<string> Check(ReorgLedger item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReorgLedgerRequiredCodeRule(string Message) : IReorgLedgerRule
{
    public List<string> Check(ReorgLedger item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a reorg ledger.</summary>
public sealed record ReorgLedgerValidator(List<IReorgLedgerRule> Rules)
{
    public List<string> Validate(ReorgLedger item)
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

    public void RequireValid(ReorgLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReorgLedger> items)
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

/// <summary>Functions of reorg ledger rules.</summary>
public static class ReorgLedgerRules
{
    public static ReorgLedgerValidator DefaultReorgLedgerValidator()
    {
        return new ReorgLedgerValidator(
            new List<IReorgLedgerRule>
            {
                new ReorgLedgerScoreRangeRule(2.4, 2034.3),
                new ReorgLedgerApprovedGuardRule(6.1),
                new ReorgLedgerRequiredNameRule("reorg ledger needs this field"),
                new ReorgLedgerRequiredTitleRule("reorg ledger needs an entry"),
                new ReorgLedgerRequiredCodeRule("reorg ledger needs an entry"),
            });
    }
}
