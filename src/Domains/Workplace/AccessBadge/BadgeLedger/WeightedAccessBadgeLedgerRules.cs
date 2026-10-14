namespace Acme.Workforce.Workplace.AccessBadge.BadgeLedger;

/// <summary>A rule a weighted access badge ledger must pass. Check returns its problems, or none.</summary>
public interface IWeightedAccessBadgeLedgerRule
{
    List<string> Check(WeightedAccessBadgeLedger item);
}

public sealed record WeightedAccessBadgeLedgerCodePrefixRule(string Prefix) : IWeightedAccessBadgeLedgerRule
{
    public List<string> Check(WeightedAccessBadgeLedger item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedAccessBadgeLedgerRetriesRangeRule(int Min, int Max) : IWeightedAccessBadgeLedgerRule
{
    public List<string> Check(WeightedAccessBadgeLedger item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedAccessBadgeLedgerRequiredTimezoneRule(string Message) : IWeightedAccessBadgeLedgerRule
{
    public List<string> Check(WeightedAccessBadgeLedger item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedAccessBadgeLedgerCodeLengthRule(int MaxLength) : IWeightedAccessBadgeLedgerRule
{
    public List<string> Check(WeightedAccessBadgeLedger item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted access badge ledger.</summary>
public sealed record WeightedAccessBadgeLedgerValidator(List<IWeightedAccessBadgeLedgerRule> Rules)
{
    public List<string> Validate(WeightedAccessBadgeLedger item)
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

    public void RequireValid(WeightedAccessBadgeLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedAccessBadgeLedger> items)
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

/// <summary>Functions of weighted access badge ledger rules.</summary>
public static class WeightedAccessBadgeLedgerRules
{
    public static WeightedAccessBadgeLedgerValidator DefaultWeightedAccessBadgeLedgerValidator()
    {
        return new WeightedAccessBadgeLedgerValidator(
            new List<IWeightedAccessBadgeLedgerRule>
            {
                new WeightedAccessBadgeLedgerCodePrefixRule("X"),
                new WeightedAccessBadgeLedgerRetriesRangeRule(32, 86400),
                new WeightedAccessBadgeLedgerRequiredTimezoneRule("weighted access badge ledger needs this field"),
                new WeightedAccessBadgeLedgerCodeLengthRule(255),
            });
    }
}
