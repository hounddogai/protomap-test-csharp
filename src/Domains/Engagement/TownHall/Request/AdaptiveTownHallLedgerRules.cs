namespace Acme.Workforce.Engagement.TownHall.Request;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a adaptive town hall ledger must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveTownHallLedgerRule
{
    List<string> Check(AdaptiveTownHallLedger item);
}

public sealed record AdaptiveTownHallLedgerTitleLengthRule(int MaxLength) : IAdaptiveTownHallLedgerRule
{
    public List<string> Check(AdaptiveTownHallLedger item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveTownHallLedgerRequiredTitleRule(string Message) : IAdaptiveTownHallLedgerRule
{
    public List<string> Check(AdaptiveTownHallLedger item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveTownHallLedgerApprovedGuardRule(double Limit) : IAdaptiveTownHallLedgerRule
{
    public List<string> Check(AdaptiveTownHallLedger item)
    {
        if (item.Approved && item.Score > Limit)
        {
            return new List<string> { $"score {item.Score} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveTownHallLedgerAllowedSummaryRule(List<string> Allowed) : IAdaptiveTownHallLedgerRule
{
    public List<string> Check(AdaptiveTownHallLedger item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Summary)))
        {
            return new List<string> { $"summary {item.Summary} is not allowed" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive town hall ledger.</summary>
public sealed record AdaptiveTownHallLedgerValidator(List<IAdaptiveTownHallLedgerRule> Rules)
{
    public List<string> Validate(AdaptiveTownHallLedger item)
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

    public void RequireValid(AdaptiveTownHallLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveTownHallLedger> items)
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

/// <summary>Functions of adaptive town hall ledger rules.</summary>
public static class AdaptiveTownHallLedgerRules
{
    public static AdaptiveTownHallLedgerValidator DefaultAdaptiveTownHallLedgerValidator()
    {
        return new AdaptiveTownHallLedgerValidator(
            new List<IAdaptiveTownHallLedgerRule>
            {
                new AdaptiveTownHallLedgerTitleLengthRule(120),
                new AdaptiveTownHallLedgerRequiredTitleRule("adaptive town hall ledger needs an entry"),
                new AdaptiveTownHallLedgerApprovedGuardRule(1.0),
                new AdaptiveTownHallLedgerAllowedSummaryRule(new List<string> { "weighted" }),
            });
    }
}
