namespace Acme.Workforce.Absence.Balance.Segment;

/// <summary>A rule a balance snapshot must pass. Check returns its problems, or none.</summary>
public interface IBalanceSnapshotRule
{
    List<string> Check(BalanceSnapshot item);
}

public sealed record BalanceSnapshotCapacityRangeRule(int Min, int Max) : IBalanceSnapshotRule
{
    public List<string> Check(BalanceSnapshot item)
    {
        var valueValue = item.Capacity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"capacity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceSnapshotCodePrefixRule(string Prefix) : IBalanceSnapshotRule
{
    public List<string> Check(BalanceSnapshot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceSnapshotAmountRangeRule(double Min, double Max) : IBalanceSnapshotRule
{
    public List<string> Check(BalanceSnapshot item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceSnapshotRequiredEmployeeIdRule(string Message) : IBalanceSnapshotRule
{
    public List<string> Check(BalanceSnapshot item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a balance snapshot.</summary>
public sealed record BalanceSnapshotValidator(List<IBalanceSnapshotRule> Rules)
{
    public List<string> Validate(BalanceSnapshot item)
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

    public void RequireValid(BalanceSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BalanceSnapshot> items)
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

/// <summary>Functions of balance snapshot rules.</summary>
public static class BalanceSnapshotRules
{
    public static BalanceSnapshotValidator DefaultBalanceSnapshotValidator()
    {
        return new BalanceSnapshotValidator(
            new List<IBalanceSnapshotRule>
            {
                new BalanceSnapshotCapacityRangeRule(80, 1000),
                new BalanceSnapshotCodePrefixRule("REF"),
                new BalanceSnapshotAmountRangeRule(8.7, 480.7),
                new BalanceSnapshotRequiredEmployeeIdRule("balance snapshot needs a value"),
            });
    }
}
