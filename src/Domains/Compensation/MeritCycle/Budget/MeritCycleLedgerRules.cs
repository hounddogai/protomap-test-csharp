namespace Acme.Workforce.Compensation.MeritCycle.Budget;

/// <summary>A rule a merit cycle ledger must pass. Check returns its problems, or none.</summary>
public interface IMeritCycleLedgerRule
{
    List<string> Check(MeritCycleLedger item);
}

public sealed record MeritCycleLedgerTimezoneLengthRule(int MaxLength) : IMeritCycleLedgerRule
{
    public List<string> Check(MeritCycleLedger item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleLedgerRequiredCodeRule(string Message) : IMeritCycleLedgerRule
{
    public List<string> Check(MeritCycleLedger item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleLedgerTenureMonthsRangeRule(int Min, int Max) : IMeritCycleLedgerRule
{
    public List<string> Check(MeritCycleLedger item)
    {
        var valueValue = item.TenureMonths;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"tenure months {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleLedgerRequiredSourceRule(string Message) : IMeritCycleLedgerRule
{
    public List<string> Check(MeritCycleLedger item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleLedgerCodeLengthRule(int MaxLength) : IMeritCycleLedgerRule
{
    public List<string> Check(MeritCycleLedger item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleLedgerCodePrefixRule(string Prefix) : IMeritCycleLedgerRule
{
    public List<string> Check(MeritCycleLedger item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a merit cycle ledger.</summary>
public sealed record MeritCycleLedgerValidator(List<IMeritCycleLedgerRule> Rules)
{
    public List<string> Validate(MeritCycleLedger item)
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

    public void RequireValid(MeritCycleLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MeritCycleLedger> items)
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

/// <summary>Functions of merit cycle ledger rules.</summary>
public static class MeritCycleLedgerRules
{
    public static MeritCycleLedgerValidator DefaultMeritCycleLedgerValidator()
    {
        return new MeritCycleLedgerValidator(
            new List<IMeritCycleLedgerRule>
            {
                new MeritCycleLedgerTimezoneLengthRule(80),
                new MeritCycleLedgerRequiredCodeRule("merit cycle ledger needs an entry"),
                new MeritCycleLedgerTenureMonthsRangeRule(64, 86400),
                new MeritCycleLedgerRequiredSourceRule("merit cycle ledger needs an entry"),
                new MeritCycleLedgerCodeLengthRule(16),
                new MeritCycleLedgerCodePrefixRule("ACM"),
            });
    }
}
