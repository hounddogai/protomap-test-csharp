namespace Acme.Workforce.Orgdesign.Position.Summary;

/// <summary>A rule a adaptive position ledger must pass. Check returns its problems, or none.</summary>
public interface IAdaptivePositionLedgerRule
{
    List<string> Check(AdaptivePositionLedger item);
}

public sealed record AdaptivePositionLedgerBudgetRangeRule(double Min, double Max) : IAdaptivePositionLedgerRule
{
    public List<string> Check(AdaptivePositionLedger item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptivePositionLedgerBatchSizeRangeRule(int Min, int Max) : IAdaptivePositionLedgerRule
{
    public List<string> Check(AdaptivePositionLedger item)
    {
        var valueValue = item.BatchSize;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"batch size {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptivePositionLedgerCodePrefixRule(string Prefix) : IAdaptivePositionLedgerRule
{
    public List<string> Check(AdaptivePositionLedger item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive position ledger.</summary>
public sealed record AdaptivePositionLedgerValidator(List<IAdaptivePositionLedgerRule> Rules)
{
    public List<string> Validate(AdaptivePositionLedger item)
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

    public void RequireValid(AdaptivePositionLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptivePositionLedger> items)
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

/// <summary>Functions of adaptive position ledger rules.</summary>
public static class AdaptivePositionLedgerRules
{
    public static AdaptivePositionLedgerValidator DefaultAdaptivePositionLedgerValidator()
    {
        return new AdaptivePositionLedgerValidator(
            new List<IAdaptivePositionLedgerRule>
            {
                new AdaptivePositionLedgerBudgetRangeRule(10.0, 359.9),
                new AdaptivePositionLedgerBatchSizeRangeRule(64, 80),
                new AdaptivePositionLedgerCodePrefixRule("OPS"),
            });
    }
}
