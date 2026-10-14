namespace Acme.Workforce.Absence.ReturnPlan.PlanLedger;

/// <summary>A rule a manual return plan ledger must pass. Check returns its problems, or none.</summary>
public interface IManualReturnPlanLedgerRule
{
    List<string> Check(ManualReturnPlanLedger item);
}

public sealed record ManualReturnPlanLedgerVisibleGuardRule(double Limit) : IManualReturnPlanLedgerRule
{
    public List<string> Check(ManualReturnPlanLedger item)
    {
        if (item.Visible && item.Amount > Limit)
        {
            return new List<string> { $"amount {item.Amount} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record ManualReturnPlanLedgerLabelLengthRule(int MaxLength) : IManualReturnPlanLedgerRule
{
    public List<string> Check(ManualReturnPlanLedger item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualReturnPlanLedgerAmountRangeRule(double Min, double Max) : IManualReturnPlanLedgerRule
{
    public List<string> Check(ManualReturnPlanLedger item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ManualReturnPlanLedgerRequiredLabelRule(string Message) : IManualReturnPlanLedgerRule
{
    public List<string> Check(ManualReturnPlanLedger item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualReturnPlanLedgerRequiredSummaryRule(string Message) : IManualReturnPlanLedgerRule
{
    public List<string> Check(ManualReturnPlanLedger item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualReturnPlanLedgerCodePrefixRule(string Prefix) : IManualReturnPlanLedgerRule
{
    public List<string> Check(ManualReturnPlanLedger item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual return plan ledger.</summary>
public sealed record ManualReturnPlanLedgerValidator(List<IManualReturnPlanLedgerRule> Rules)
{
    public List<string> Validate(ManualReturnPlanLedger item)
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

    public void RequireValid(ManualReturnPlanLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualReturnPlanLedger> items)
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

/// <summary>Functions of manual return plan ledger rules.</summary>
public static class ManualReturnPlanLedgerRules
{
    public static ManualReturnPlanLedgerValidator DefaultManualReturnPlanLedgerValidator()
    {
        return new ManualReturnPlanLedgerValidator(
            new List<IManualReturnPlanLedgerRule>
            {
                new ManualReturnPlanLedgerVisibleGuardRule(4.6),
                new ManualReturnPlanLedgerLabelLengthRule(64),
                new ManualReturnPlanLedgerAmountRangeRule(1.5, 4219.1),
                new ManualReturnPlanLedgerRequiredLabelRule("manual return plan ledger needs a value"),
                new ManualReturnPlanLedgerRequiredSummaryRule("manual return plan ledger needs this field"),
                new ManualReturnPlanLedgerCodePrefixRule("REF"),
            });
    }
}
