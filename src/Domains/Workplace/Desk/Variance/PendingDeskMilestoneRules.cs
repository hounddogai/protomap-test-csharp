namespace Acme.Workforce.Workplace.Desk.Variance;

/// <summary>A rule a pending desk milestone must pass. Check returns its problems, or none.</summary>
public interface IPendingDeskMilestoneRule
{
    List<string> Check(PendingDeskMilestone item);
}

public sealed record PendingDeskMilestoneRequiredCodeRule(string Message) : IPendingDeskMilestoneRule
{
    public List<string> Check(PendingDeskMilestone item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingDeskMilestoneArchivedGuardRule(int Limit) : IPendingDeskMilestoneRule
{
    public List<string> Check(PendingDeskMilestone item)
    {
        if (item.Archived && item.Limit > Limit)
        {
            return new List<string> { $"limit {item.Limit} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record PendingDeskMilestoneCodePrefixRule(string Prefix) : IPendingDeskMilestoneRule
{
    public List<string> Check(PendingDeskMilestone item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PendingDeskMilestoneRequiredSummaryRule(string Message) : IPendingDeskMilestoneRule
{
    public List<string> Check(PendingDeskMilestone item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending desk milestone.</summary>
public sealed record PendingDeskMilestoneValidator(List<IPendingDeskMilestoneRule> Rules)
{
    public List<string> Validate(PendingDeskMilestone item)
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

    public void RequireValid(PendingDeskMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingDeskMilestone> items)
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

/// <summary>Functions of pending desk milestone rules.</summary>
public static class PendingDeskMilestoneRules
{
    public static PendingDeskMilestoneValidator DefaultPendingDeskMilestoneValidator()
    {
        return new PendingDeskMilestoneValidator(
            new List<IPendingDeskMilestoneRule>
            {
                new PendingDeskMilestoneRequiredCodeRule("pending desk milestone needs an entry"),
                new PendingDeskMilestoneArchivedGuardRule(16),
                new PendingDeskMilestoneCodePrefixRule("OPS"),
                new PendingDeskMilestoneRequiredSummaryRule("pending desk milestone needs this field"),
            });
    }
}
