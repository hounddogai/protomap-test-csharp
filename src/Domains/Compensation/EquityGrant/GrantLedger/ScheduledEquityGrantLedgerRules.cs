namespace Acme.Workforce.Compensation.EquityGrant.GrantLedger;

/// <summary>A rule a scheduled equity grant ledger must pass. Check returns its problems, or none.</summary>
public interface IScheduledEquityGrantLedgerRule
{
    List<string> Check(ScheduledEquityGrantLedger item);
}

public sealed record ScheduledEquityGrantLedgerLocaleLengthRule(int MaxLength) : IScheduledEquityGrantLedgerRule
{
    public List<string> Check(ScheduledEquityGrantLedger item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledEquityGrantLedgerLabelLengthRule(int MaxLength) : IScheduledEquityGrantLedgerRule
{
    public List<string> Check(ScheduledEquityGrantLedger item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledEquityGrantLedgerRequiredCodeRule(string Message) : IScheduledEquityGrantLedgerRule
{
    public List<string> Check(ScheduledEquityGrantLedger item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledEquityGrantLedgerCodePrefixRule(string Prefix) : IScheduledEquityGrantLedgerRule
{
    public List<string> Check(ScheduledEquityGrantLedger item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled equity grant ledger.</summary>
public sealed record ScheduledEquityGrantLedgerValidator(List<IScheduledEquityGrantLedgerRule> Rules)
{
    public List<string> Validate(ScheduledEquityGrantLedger item)
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

    public void RequireValid(ScheduledEquityGrantLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledEquityGrantLedger> items)
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

/// <summary>Functions of scheduled equity grant ledger rules.</summary>
public static class ScheduledEquityGrantLedgerRules
{
    public static ScheduledEquityGrantLedgerValidator DefaultScheduledEquityGrantLedgerValidator()
    {
        return new ScheduledEquityGrantLedgerValidator(
            new List<IScheduledEquityGrantLedgerRule>
            {
                new ScheduledEquityGrantLedgerLocaleLengthRule(16),
                new ScheduledEquityGrantLedgerLabelLengthRule(120),
                new ScheduledEquityGrantLedgerRequiredCodeRule("scheduled equity grant ledger needs a value"),
                new ScheduledEquityGrantLedgerCodePrefixRule("ACM"),
            });
    }
}
