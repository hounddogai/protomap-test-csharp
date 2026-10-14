namespace Acme.Workforce.Talent.Pool.Group;

/// <summary>A rule a pool ledger must pass. Check returns its problems, or none.</summary>
public interface IPoolLedgerRule
{
    List<string> Check(PoolLedger item);
}

public sealed record PoolLedgerLabelLengthRule(int MaxLength) : IPoolLedgerRule
{
    public List<string> Check(PoolLedger item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PoolLedgerRequiredCodeRule(string Message) : IPoolLedgerRule
{
    public List<string> Check(PoolLedger item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PoolLedgerRequiredLabelRule(string Message) : IPoolLedgerRule
{
    public List<string> Check(PoolLedger item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PoolLedgerSegmentLengthRule(int MaxLength) : IPoolLedgerRule
{
    public List<string> Check(PoolLedger item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pool ledger.</summary>
public sealed record PoolLedgerValidator(List<IPoolLedgerRule> Rules)
{
    public List<string> Validate(PoolLedger item)
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

    public void RequireValid(PoolLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PoolLedger> items)
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

/// <summary>Functions of pool ledger rules.</summary>
public static class PoolLedgerRules
{
    public static PoolLedgerValidator DefaultPoolLedgerValidator()
    {
        return new PoolLedgerValidator(
            new List<IPoolLedgerRule>
            {
                new PoolLedgerLabelLengthRule(80),
                new PoolLedgerRequiredCodeRule("pool ledger needs a value"),
                new PoolLedgerRequiredLabelRule("pool ledger needs this field"),
                new PoolLedgerSegmentLengthRule(1000),
            });
    }
}
