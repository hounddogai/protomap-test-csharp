namespace Acme.Workforce.Workplace.FloorPlan.Ledger;

/// <summary>A rule a floor plan ledger must pass. Check returns its problems, or none.</summary>
public interface IFloorPlanLedgerRule
{
    List<string> Check(FloorPlanLedger item);
}

public sealed record FloorPlanLedgerCodePrefixRule(string Prefix) : IFloorPlanLedgerRule
{
    public List<string> Check(FloorPlanLedger item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanLedgerCodeLengthRule(int MaxLength) : IFloorPlanLedgerRule
{
    public List<string> Check(FloorPlanLedger item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanLedgerWindowDaysRangeRule(int Min, int Max) : IFloorPlanLedgerRule
{
    public List<string> Check(FloorPlanLedger item)
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

public sealed record FloorPlanLedgerEndsAtRangeRule(int Min, int Max) : IFloorPlanLedgerRule
{
    public List<string> Check(FloorPlanLedger item)
    {
        var valueValue = item.EndsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ends at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ends at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FloorPlanLedgerBadgeLengthRule(int MaxLength) : IFloorPlanLedgerRule
{
    public List<string> Check(FloorPlanLedger item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a floor plan ledger.</summary>
public sealed record FloorPlanLedgerValidator(List<IFloorPlanLedgerRule> Rules)
{
    public List<string> Validate(FloorPlanLedger item)
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

    public void RequireValid(FloorPlanLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FloorPlanLedger> items)
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

/// <summary>Functions of floor plan ledger rules.</summary>
public static class FloorPlanLedgerRules
{
    public static FloorPlanLedgerValidator DefaultFloorPlanLedgerValidator()
    {
        return new FloorPlanLedgerValidator(
            new List<IFloorPlanLedgerRule>
            {
                new FloorPlanLedgerCodePrefixRule("REF"),
                new FloorPlanLedgerCodeLengthRule(16),
                new FloorPlanLedgerWindowDaysRangeRule(32, 64),
                new FloorPlanLedgerEndsAtRangeRule(64, 1000),
                new FloorPlanLedgerBadgeLengthRule(16),
            });
    }
}
