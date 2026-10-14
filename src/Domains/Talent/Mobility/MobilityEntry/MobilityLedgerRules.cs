namespace Acme.Workforce.Talent.Mobility.MobilityEntry;

/// <summary>A rule a mobility ledger must pass. Check returns its problems, or none.</summary>
public interface IMobilityLedgerRule
{
    List<string> Check(MobilityLedger item);
}

public sealed record MobilityLedgerRequiredLocaleRule(string Message) : IMobilityLedgerRule
{
    public List<string> Check(MobilityLedger item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MobilityLedgerTitleLengthRule(int MaxLength) : IMobilityLedgerRule
{
    public List<string> Check(MobilityLedger item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MobilityLedgerCodePrefixRule(string Prefix) : IMobilityLedgerRule
{
    public List<string> Check(MobilityLedger item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record MobilityLedgerRequiredNotesRule(string Message) : IMobilityLedgerRule
{
    public List<string> Check(MobilityLedger item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a mobility ledger.</summary>
public sealed record MobilityLedgerValidator(List<IMobilityLedgerRule> Rules)
{
    public List<string> Validate(MobilityLedger item)
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

    public void RequireValid(MobilityLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MobilityLedger> items)
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

/// <summary>Functions of mobility ledger rules.</summary>
public static class MobilityLedgerRules
{
    public static MobilityLedgerValidator DefaultMobilityLedgerValidator()
    {
        return new MobilityLedgerValidator(
            new List<IMobilityLedgerRule>
            {
                new MobilityLedgerRequiredLocaleRule("mobility ledger needs this field"),
                new MobilityLedgerTitleLengthRule(80),
                new MobilityLedgerCodePrefixRule("REF"),
                new MobilityLedgerRequiredNotesRule("mobility ledger needs this field"),
            });
    }
}
