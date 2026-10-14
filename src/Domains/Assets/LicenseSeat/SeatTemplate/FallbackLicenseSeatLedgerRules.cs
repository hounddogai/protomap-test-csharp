namespace Acme.Workforce.Assets.LicenseSeat.SeatTemplate;

/// <summary>A rule a fallback license seat ledger must pass. Check returns its problems, or none.</summary>
public interface IFallbackLicenseSeatLedgerRule
{
    List<string> Check(FallbackLicenseSeatLedger item);
}

public sealed record FallbackLicenseSeatLedgerSegmentLengthRule(int MaxLength) : IFallbackLicenseSeatLedgerRule
{
    public List<string> Check(FallbackLicenseSeatLedger item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackLicenseSeatLedgerRequiredSegmentRule(string Message) : IFallbackLicenseSeatLedgerRule
{
    public List<string> Check(FallbackLicenseSeatLedger item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackLicenseSeatLedgerApprovedGuardRule(int Limit) : IFallbackLicenseSeatLedgerRule
{
    public List<string> Check(FallbackLicenseSeatLedger item)
    {
        if (item.Approved && item.Sequence > Limit)
        {
            return new List<string> { $"sequence {item.Sequence} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record FallbackLicenseSeatLedgerChecksumLengthRule(int MaxLength) : IFallbackLicenseSeatLedgerRule
{
    public List<string> Check(FallbackLicenseSeatLedger item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackLicenseSeatLedgerVersionRangeRule(int Min, int Max) : IFallbackLicenseSeatLedgerRule
{
    public List<string> Check(FallbackLicenseSeatLedger item)
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

public sealed record FallbackLicenseSeatLedgerRequiredCodeRule(string Message) : IFallbackLicenseSeatLedgerRule
{
    public List<string> Check(FallbackLicenseSeatLedger item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback license seat ledger.</summary>
public sealed record FallbackLicenseSeatLedgerValidator(List<IFallbackLicenseSeatLedgerRule> Rules)
{
    public List<string> Validate(FallbackLicenseSeatLedger item)
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

    public void RequireValid(FallbackLicenseSeatLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackLicenseSeatLedger> items)
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

/// <summary>Functions of fallback license seat ledger rules.</summary>
public static class FallbackLicenseSeatLedgerRules
{
    public static FallbackLicenseSeatLedgerValidator DefaultFallbackLicenseSeatLedgerValidator()
    {
        return new FallbackLicenseSeatLedgerValidator(
            new List<IFallbackLicenseSeatLedgerRule>
            {
                new FallbackLicenseSeatLedgerSegmentLengthRule(120),
                new FallbackLicenseSeatLedgerRequiredSegmentRule("fallback license seat ledger needs an entry"),
                new FallbackLicenseSeatLedgerApprovedGuardRule(80),
                new FallbackLicenseSeatLedgerChecksumLengthRule(3600),
                new FallbackLicenseSeatLedgerVersionRangeRule(32, 120),
                new FallbackLicenseSeatLedgerRequiredCodeRule("fallback license seat ledger needs an entry"),
            });
    }
}
