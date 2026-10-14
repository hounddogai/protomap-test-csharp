namespace Acme.Workforce.Absence.Policy.PolicyEstimate;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pending policy adjustment must pass. Check returns its problems, or none.</summary>
public interface IPendingPolicyAdjustmentRule
{
    List<string> Check(PendingPolicyAdjustment item);
}

public sealed record PendingPolicyAdjustmentRequiredTitleRule(string Message) : IPendingPolicyAdjustmentRule
{
    public List<string> Check(PendingPolicyAdjustment item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingPolicyAdjustmentLocationLengthRule(int MaxLength) : IPendingPolicyAdjustmentRule
{
    public List<string> Check(PendingPolicyAdjustment item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingPolicyAdjustmentAllowedTitleRule(List<string> Allowed) : IPendingPolicyAdjustmentRule
{
    public List<string> Check(PendingPolicyAdjustment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is stale" };
        }
        return new List<string>();
    }
}

public sealed record PendingPolicyAdjustmentRequiredCodeRule(string Message) : IPendingPolicyAdjustmentRule
{
    public List<string> Check(PendingPolicyAdjustment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending policy adjustment.</summary>
public sealed record PendingPolicyAdjustmentValidator(List<IPendingPolicyAdjustmentRule> Rules)
{
    public List<string> Validate(PendingPolicyAdjustment item)
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

    public void RequireValid(PendingPolicyAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingPolicyAdjustment> items)
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

/// <summary>Functions of pending policy adjustment rules.</summary>
public static class PendingPolicyAdjustmentRules
{
    public static PendingPolicyAdjustmentValidator DefaultPendingPolicyAdjustmentValidator()
    {
        return new PendingPolicyAdjustmentValidator(
            new List<IPendingPolicyAdjustmentRule>
            {
                new PendingPolicyAdjustmentRequiredTitleRule("pending policy adjustment needs an entry"),
                new PendingPolicyAdjustmentLocationLengthRule(255),
                new PendingPolicyAdjustmentAllowedTitleRule(new List<string> { "fallback", "strict" }),
                new PendingPolicyAdjustmentRequiredCodeRule("pending policy adjustment needs a value"),
            });
    }
}
