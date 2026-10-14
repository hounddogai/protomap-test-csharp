namespace Acme.Workforce.Compensation.Adjustment.AdjustmentRule;

/// <summary>A rule a adjustment ledger must pass. Check returns its problems, or none.</summary>
public interface IAdjustmentLedgerRule
{
    List<string> Check(AdjustmentLedger item);
}

public sealed record AdjustmentLedgerRequiredNameRule(string Message) : IAdjustmentLedgerRule
{
    public List<string> Check(AdjustmentLedger item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentLedgerLeaveDaysRangeRule(int Min, int Max) : IAdjustmentLedgerRule
{
    public List<string> Check(AdjustmentLedger item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentLedgerRegionLengthRule(int MaxLength) : IAdjustmentLedgerRule
{
    public List<string> Check(AdjustmentLedger item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adjustment ledger.</summary>
public sealed record AdjustmentLedgerValidator(List<IAdjustmentLedgerRule> Rules)
{
    public List<string> Validate(AdjustmentLedger item)
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

    public void RequireValid(AdjustmentLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdjustmentLedger> items)
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

/// <summary>Functions of adjustment ledger rules.</summary>
public static class AdjustmentLedgerRules
{
    public static AdjustmentLedgerValidator DefaultAdjustmentLedgerValidator()
    {
        return new AdjustmentLedgerValidator(
            new List<IAdjustmentLedgerRule>
            {
                new AdjustmentLedgerRequiredNameRule("adjustment ledger needs a value"),
                new AdjustmentLedgerLeaveDaysRangeRule(16, 80),
                new AdjustmentLedgerRegionLengthRule(86400),
            });
    }
}
