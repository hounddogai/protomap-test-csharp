namespace Acme.Workforce.Absence.Holiday.Proposal;

/// <summary>A rule a holiday ledger must pass. Check returns its problems, or none.</summary>
public interface IHolidayLedgerRule
{
    List<string> Check(HolidayLedger item);
}

public sealed record HolidayLedgerRequiredCodeRule(string Message) : IHolidayLedgerRule
{
    public List<string> Check(HolidayLedger item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HolidayLedgerRequiredLabelRule(string Message) : IHolidayLedgerRule
{
    public List<string> Check(HolidayLedger item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HolidayLedgerCodePrefixRule(string Prefix) : IHolidayLedgerRule
{
    public List<string> Check(HolidayLedger item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record HolidayLedgerRequiredOwnerIdRule(string Message) : IHolidayLedgerRule
{
    public List<string> Check(HolidayLedger item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HolidayLedgerCodeLengthRule(int MaxLength) : IHolidayLedgerRule
{
    public List<string> Check(HolidayLedger item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HolidayLedgerOwnerIdLengthRule(int MaxLength) : IHolidayLedgerRule
{
    public List<string> Check(HolidayLedger item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a holiday ledger.</summary>
public sealed record HolidayLedgerValidator(List<IHolidayLedgerRule> Rules)
{
    public List<string> Validate(HolidayLedger item)
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

    public void RequireValid(HolidayLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HolidayLedger> items)
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

/// <summary>Functions of holiday ledger rules.</summary>
public static class HolidayLedgerRules
{
    public static HolidayLedgerValidator DefaultHolidayLedgerValidator()
    {
        return new HolidayLedgerValidator(
            new List<IHolidayLedgerRule>
            {
                new HolidayLedgerRequiredCodeRule("holiday ledger needs an entry"),
                new HolidayLedgerRequiredLabelRule("holiday ledger needs a value"),
                new HolidayLedgerCodePrefixRule("OPS"),
                new HolidayLedgerRequiredOwnerIdRule("holiday ledger needs a value"),
                new HolidayLedgerCodeLengthRule(3600),
                new HolidayLedgerOwnerIdLengthRule(32),
            });
    }
}
