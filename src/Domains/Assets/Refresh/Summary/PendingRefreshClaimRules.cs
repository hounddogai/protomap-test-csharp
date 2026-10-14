namespace Acme.Workforce.Assets.Refresh.Summary;

/// <summary>A rule a pending refresh claim must pass. Check returns its problems, or none.</summary>
public interface IPendingRefreshClaimRule
{
    List<string> Check(PendingRefreshClaim item);
}

public sealed record PendingRefreshClaimRateRangeRule(double Min, double Max) : IPendingRefreshClaimRule
{
    public List<string> Check(PendingRefreshClaim item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PendingRefreshClaimRequiredTitleRule(string Message) : IPendingRefreshClaimRule
{
    public List<string> Check(PendingRefreshClaim item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingRefreshClaimRequiredLabelRule(string Message) : IPendingRefreshClaimRule
{
    public List<string> Check(PendingRefreshClaim item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending refresh claim.</summary>
public sealed record PendingRefreshClaimValidator(List<IPendingRefreshClaimRule> Rules)
{
    public List<string> Validate(PendingRefreshClaim item)
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

    public void RequireValid(PendingRefreshClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingRefreshClaim> items)
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

/// <summary>Functions of pending refresh claim rules.</summary>
public static class PendingRefreshClaimRules
{
    public static PendingRefreshClaimValidator DefaultPendingRefreshClaimValidator()
    {
        return new PendingRefreshClaimValidator(
            new List<IPendingRefreshClaimRule>
            {
                new PendingRefreshClaimRateRangeRule(1.0, 4052.9),
                new PendingRefreshClaimRequiredTitleRule("pending refresh claim needs an entry"),
                new PendingRefreshClaimRequiredLabelRule("pending refresh claim needs a value"),
            });
    }
}
