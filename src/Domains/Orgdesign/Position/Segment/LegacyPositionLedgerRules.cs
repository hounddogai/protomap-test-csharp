namespace Acme.Workforce.Orgdesign.Position.Segment;

/// <summary>A rule a legacy position ledger must pass. Check returns its problems, or none.</summary>
public interface ILegacyPositionLedgerRule
{
    List<string> Check(LegacyPositionLedger item);
}

public sealed record LegacyPositionLedgerRequiredTitleRule(string Message) : ILegacyPositionLedgerRule
{
    public List<string> Check(LegacyPositionLedger item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyPositionLedgerChecksumLengthRule(int MaxLength) : ILegacyPositionLedgerRule
{
    public List<string> Check(LegacyPositionLedger item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyPositionLedgerCodeLengthRule(int MaxLength) : ILegacyPositionLedgerRule
{
    public List<string> Check(LegacyPositionLedger item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyPositionLedgerWindowDaysRangeRule(int Min, int Max) : ILegacyPositionLedgerRule
{
    public List<string> Check(LegacyPositionLedger item)
    {
        var valueValue = item.WindowDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"window days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"window days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LegacyPositionLedgerCodePrefixRule(string Prefix) : ILegacyPositionLedgerRule
{
    public List<string> Check(LegacyPositionLedger item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyPositionLedgerBadgeLengthRule(int MaxLength) : ILegacyPositionLedgerRule
{
    public List<string> Check(LegacyPositionLedger item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy position ledger.</summary>
public sealed record LegacyPositionLedgerValidator(List<ILegacyPositionLedgerRule> Rules)
{
    public List<string> Validate(LegacyPositionLedger item)
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

    public void RequireValid(LegacyPositionLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyPositionLedger> items)
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

/// <summary>Functions of legacy position ledger rules.</summary>
public static class LegacyPositionLedgerRules
{
    public static LegacyPositionLedgerValidator DefaultLegacyPositionLedgerValidator()
    {
        return new LegacyPositionLedgerValidator(
            new List<ILegacyPositionLedgerRule>
            {
                new LegacyPositionLedgerRequiredTitleRule("legacy position ledger needs an entry"),
                new LegacyPositionLedgerChecksumLengthRule(80),
                new LegacyPositionLedgerCodeLengthRule(120),
                new LegacyPositionLedgerWindowDaysRangeRule(64, 3600),
                new LegacyPositionLedgerCodePrefixRule("Q"),
                new LegacyPositionLedgerBadgeLengthRule(64),
            });
    }
}
