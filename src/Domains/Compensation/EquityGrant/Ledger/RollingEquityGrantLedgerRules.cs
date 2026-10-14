namespace Acme.Workforce.Compensation.EquityGrant.Ledger;

/// <summary>A rule a rolling equity grant ledger must pass. Check returns its problems, or none.</summary>
public interface IRollingEquityGrantLedgerRule
{
    List<string> Check(RollingEquityGrantLedger item);
}

public sealed record RollingEquityGrantLedgerCodePrefixRule(string Prefix) : IRollingEquityGrantLedgerRule
{
    public List<string> Check(RollingEquityGrantLedger item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RollingEquityGrantLedgerRequiredCodeRule(string Message) : IRollingEquityGrantLedgerRule
{
    public List<string> Check(RollingEquityGrantLedger item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingEquityGrantLedgerActiveGuardRule(int Limit) : IRollingEquityGrantLedgerRule
{
    public List<string> Check(RollingEquityGrantLedger item)
    {
        if (item.Active && item.Limit > Limit)
        {
            return new List<string> { $"limit {item.Limit} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record RollingEquityGrantLedgerRequiredTitleRule(string Message) : IRollingEquityGrantLedgerRule
{
    public List<string> Check(RollingEquityGrantLedger item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingEquityGrantLedgerCodeLengthRule(int MaxLength) : IRollingEquityGrantLedgerRule
{
    public List<string> Check(RollingEquityGrantLedger item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingEquityGrantLedgerLimitRangeRule(int Min, int Max) : IRollingEquityGrantLedgerRule
{
    public List<string> Check(RollingEquityGrantLedger item)
    {
        var valueValue = item.Limit;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"limit {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"limit {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a rolling equity grant ledger.</summary>
public sealed record RollingEquityGrantLedgerValidator(List<IRollingEquityGrantLedgerRule> Rules)
{
    public List<string> Validate(RollingEquityGrantLedger item)
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

    public void RequireValid(RollingEquityGrantLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingEquityGrantLedger> items)
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

/// <summary>Functions of rolling equity grant ledger rules.</summary>
public static class RollingEquityGrantLedgerRules
{
    public static RollingEquityGrantLedgerValidator DefaultRollingEquityGrantLedgerValidator()
    {
        return new RollingEquityGrantLedgerValidator(
            new List<IRollingEquityGrantLedgerRule>
            {
                new RollingEquityGrantLedgerCodePrefixRule("X"),
                new RollingEquityGrantLedgerRequiredCodeRule("rolling equity grant ledger needs an entry"),
                new RollingEquityGrantLedgerActiveGuardRule(86400),
                new RollingEquityGrantLedgerRequiredTitleRule("rolling equity grant ledger needs an entry"),
                new RollingEquityGrantLedgerCodeLengthRule(64),
                new RollingEquityGrantLedgerLimitRangeRule(80, 1000),
            });
    }
}
