namespace Acme.Workforce.Workplace.Occupancy.Ledger;

/// <summary>A rule a occupancy ledger must pass. Check returns its problems, or none.</summary>
public interface IOccupancyLedgerRule
{
    List<string> Check(OccupancyLedger item);
}

public sealed record OccupancyLedgerTitleLengthRule(int MaxLength) : IOccupancyLedgerRule
{
    public List<string> Check(OccupancyLedger item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyLedgerRequiredCategoryRule(string Message) : IOccupancyLedgerRule
{
    public List<string> Check(OccupancyLedger item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OccupancyLedgerCodePrefixRule(string Prefix) : IOccupancyLedgerRule
{
    public List<string> Check(OccupancyLedger item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyLedgerCodeLengthRule(int MaxLength) : IOccupancyLedgerRule
{
    public List<string> Check(OccupancyLedger item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a occupancy ledger.</summary>
public sealed record OccupancyLedgerValidator(List<IOccupancyLedgerRule> Rules)
{
    public List<string> Validate(OccupancyLedger item)
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

    public void RequireValid(OccupancyLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OccupancyLedger> items)
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

/// <summary>Functions of occupancy ledger rules.</summary>
public static class OccupancyLedgerRules
{
    public static OccupancyLedgerValidator DefaultOccupancyLedgerValidator()
    {
        return new OccupancyLedgerValidator(
            new List<IOccupancyLedgerRule>
            {
                new OccupancyLedgerTitleLengthRule(86400),
                new OccupancyLedgerRequiredCategoryRule("occupancy ledger needs this field"),
                new OccupancyLedgerCodePrefixRule("OPS"),
                new OccupancyLedgerCodeLengthRule(1000),
            });
    }
}
