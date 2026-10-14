namespace Acme.Workforce.Compensation.EquityGrant.Digest;

/// <summary>A rule a equity grant ledger must pass. Check returns its problems, or none.</summary>
public interface IEquityGrantLedgerRule
{
    List<string> Check(EquityGrantLedger item);
}

public sealed record EquityGrantLedgerLeaveDaysRangeRule(int Min, int Max) : IEquityGrantLedgerRule
{
    public List<string> Check(EquityGrantLedger item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantLedgerVisibleGuardRule(double Limit) : IEquityGrantLedgerRule
{
    public List<string> Check(EquityGrantLedger item)
    {
        if (item.Visible && item.Ceiling > Limit)
        {
            return new List<string> { $"ceiling {item.Ceiling} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantLedgerVersionRangeRule(int Min, int Max) : IEquityGrantLedgerRule
{
    public List<string> Check(EquityGrantLedger item)
    {
        var valueValue = item.Version;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"version {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"version {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record EquityGrantLedgerRequiredRegionRule(string Message) : IEquityGrantLedgerRule
{
    public List<string> Check(EquityGrantLedger item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantLedgerRequiredTitleRule(string Message) : IEquityGrantLedgerRule
{
    public List<string> Check(EquityGrantLedger item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a equity grant ledger.</summary>
public sealed record EquityGrantLedgerValidator(List<IEquityGrantLedgerRule> Rules)
{
    public List<string> Validate(EquityGrantLedger item)
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

    public void RequireValid(EquityGrantLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EquityGrantLedger> items)
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

/// <summary>Functions of equity grant ledger rules.</summary>
public static class EquityGrantLedgerRules
{
    public static EquityGrantLedgerValidator DefaultEquityGrantLedgerValidator()
    {
        return new EquityGrantLedgerValidator(
            new List<IEquityGrantLedgerRule>
            {
                new EquityGrantLedgerLeaveDaysRangeRule(16, 80),
                new EquityGrantLedgerVisibleGuardRule(8.6),
                new EquityGrantLedgerVersionRangeRule(80, 86400),
                new EquityGrantLedgerRequiredRegionRule("equity grant ledger needs an entry"),
                new EquityGrantLedgerRequiredTitleRule("equity grant ledger needs this field"),
            });
    }
}
