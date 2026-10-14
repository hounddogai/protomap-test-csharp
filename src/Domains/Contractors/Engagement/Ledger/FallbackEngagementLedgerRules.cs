namespace Acme.Workforce.Contractors.Engagement.Ledger;

/// <summary>A rule a fallback engagement ledger must pass. Check returns its problems, or none.</summary>
public interface IFallbackEngagementLedgerRule
{
    List<string> Check(FallbackEngagementLedger item);
}

public sealed record FallbackEngagementLedgerRequiredJobCodeRule(string Message) : IFallbackEngagementLedgerRule
{
    public List<string> Check(FallbackEngagementLedger item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackEngagementLedgerRequiredTitleRule(string Message) : IFallbackEngagementLedgerRule
{
    public List<string> Check(FallbackEngagementLedger item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackEngagementLedgerBudgetRangeRule(double Min, double Max) : IFallbackEngagementLedgerRule
{
    public List<string> Check(FallbackEngagementLedger item)
    {
        var valueValue = item.Budget;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"budget {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"budget {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FallbackEngagementLedgerWindowDaysRangeRule(int Min, int Max) : IFallbackEngagementLedgerRule
{
    public List<string> Check(FallbackEngagementLedger item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback engagement ledger.</summary>
public sealed record FallbackEngagementLedgerValidator(List<IFallbackEngagementLedgerRule> Rules)
{
    public List<string> Validate(FallbackEngagementLedger item)
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

    public void RequireValid(FallbackEngagementLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackEngagementLedger> items)
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

/// <summary>Functions of fallback engagement ledger rules.</summary>
public static class FallbackEngagementLedgerRules
{
    public static FallbackEngagementLedgerValidator DefaultFallbackEngagementLedgerValidator()
    {
        return new FallbackEngagementLedgerValidator(
            new List<IFallbackEngagementLedgerRule>
            {
                new FallbackEngagementLedgerRequiredJobCodeRule("fallback engagement ledger needs a value"),
                new FallbackEngagementLedgerRequiredTitleRule("fallback engagement ledger needs a value"),
                new FallbackEngagementLedgerBudgetRangeRule(7.0, 3667.2),
                new FallbackEngagementLedgerWindowDaysRangeRule(80, 120),
            });
    }
}
