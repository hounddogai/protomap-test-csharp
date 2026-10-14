namespace Acme.Workforce.Absence.Balance.Policy;

/// <summary>A rule a weighted balance snapshot must pass. Check returns its problems, or none.</summary>
public interface IWeightedBalanceSnapshotRule
{
    List<string> Check(WeightedBalanceSnapshot item);
}

public sealed record WeightedBalanceSnapshotStartsAtRangeRule(int Min, int Max) : IWeightedBalanceSnapshotRule
{
    public List<string> Check(WeightedBalanceSnapshot item)
    {
        var valueValue = item.StartsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"starts at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"starts at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record WeightedBalanceSnapshotRequiredNameRule(string Message) : IWeightedBalanceSnapshotRule
{
    public List<string> Check(WeightedBalanceSnapshot item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedBalanceSnapshotCodePrefixRule(string Prefix) : IWeightedBalanceSnapshotRule
{
    public List<string> Check(WeightedBalanceSnapshot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedBalanceSnapshotRequiredCodeRule(string Message) : IWeightedBalanceSnapshotRule
{
    public List<string> Check(WeightedBalanceSnapshot item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted balance snapshot.</summary>
public sealed record WeightedBalanceSnapshotValidator(List<IWeightedBalanceSnapshotRule> Rules)
{
    public List<string> Validate(WeightedBalanceSnapshot item)
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

    public void RequireValid(WeightedBalanceSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedBalanceSnapshot> items)
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

/// <summary>Functions of weighted balance snapshot rules.</summary>
public static class WeightedBalanceSnapshotRules
{
    public static WeightedBalanceSnapshotValidator DefaultWeightedBalanceSnapshotValidator()
    {
        return new WeightedBalanceSnapshotValidator(
            new List<IWeightedBalanceSnapshotRule>
            {
                new WeightedBalanceSnapshotStartsAtRangeRule(255, 1000),
                new WeightedBalanceSnapshotRequiredNameRule("weighted balance snapshot needs this field"),
                new WeightedBalanceSnapshotCodePrefixRule("REF"),
                new WeightedBalanceSnapshotRequiredCodeRule("weighted balance snapshot needs a value"),
            });
    }
}
