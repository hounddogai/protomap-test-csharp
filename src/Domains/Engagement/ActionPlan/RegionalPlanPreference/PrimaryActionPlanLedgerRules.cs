namespace Acme.Workforce.Engagement.ActionPlan.RegionalPlanPreference;

/// <summary>A rule a primary action plan ledger must pass. Check returns its problems, or none.</summary>
public interface IPrimaryActionPlanLedgerRule
{
    List<string> Check(PrimaryActionPlanLedger item);
}

public sealed record PrimaryActionPlanLedgerRequiredRegionRule(string Message) : IPrimaryActionPlanLedgerRule
{
    public List<string> Check(PrimaryActionPlanLedger item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryActionPlanLedgerCodePrefixRule(string Prefix) : IPrimaryActionPlanLedgerRule
{
    public List<string> Check(PrimaryActionPlanLedger item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryActionPlanLedgerRegionLengthRule(int MaxLength) : IPrimaryActionPlanLedgerRule
{
    public List<string> Check(PrimaryActionPlanLedger item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryActionPlanLedgerRequiredReviewerRule(string Message) : IPrimaryActionPlanLedgerRule
{
    public List<string> Check(PrimaryActionPlanLedger item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryActionPlanLedgerCodeLengthRule(int MaxLength) : IPrimaryActionPlanLedgerRule
{
    public List<string> Check(PrimaryActionPlanLedger item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary action plan ledger.</summary>
public sealed record PrimaryActionPlanLedgerValidator(List<IPrimaryActionPlanLedgerRule> Rules)
{
    public List<string> Validate(PrimaryActionPlanLedger item)
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

    public void RequireValid(PrimaryActionPlanLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryActionPlanLedger> items)
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

/// <summary>Functions of primary action plan ledger rules.</summary>
public static class PrimaryActionPlanLedgerRules
{
    public static PrimaryActionPlanLedgerValidator DefaultPrimaryActionPlanLedgerValidator()
    {
        return new PrimaryActionPlanLedgerValidator(
            new List<IPrimaryActionPlanLedgerRule>
            {
                new PrimaryActionPlanLedgerRequiredRegionRule("primary action plan ledger needs this field"),
                new PrimaryActionPlanLedgerCodePrefixRule("Q"),
                new PrimaryActionPlanLedgerRegionLengthRule(32),
                new PrimaryActionPlanLedgerRequiredReviewerRule("primary action plan ledger needs this field"),
                new PrimaryActionPlanLedgerCodeLengthRule(80),
            });
    }
}
