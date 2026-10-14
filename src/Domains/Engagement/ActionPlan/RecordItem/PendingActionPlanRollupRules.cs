namespace Acme.Workforce.Engagement.ActionPlan.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pending action plan rollup must pass. Check returns its problems, or none.</summary>
public interface IPendingActionPlanRollupRule
{
    List<string> Check(PendingActionPlanRollup item);
}

public sealed record PendingActionPlanRollupRequiredBadgeRule(string Message) : IPendingActionPlanRollupRule
{
    public List<string> Check(PendingActionPlanRollup item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingActionPlanRollupRetriesRangeRule(int Min, int Max) : IPendingActionPlanRollupRule
{
    public List<string> Check(PendingActionPlanRollup item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PendingActionPlanRollupBadgeLengthRule(int MaxLength) : IPendingActionPlanRollupRule
{
    public List<string> Check(PendingActionPlanRollup item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingActionPlanRollupAllowedOwnerIdRule(List<string> Allowed) : IPendingActionPlanRollupRule
{
    public List<string> Check(PendingActionPlanRollup item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.OwnerId)))
        {
            return new List<string> { $"owner id {item.OwnerId} is missing" };
        }
        return new List<string>();
    }
}

public sealed record PendingActionPlanRollupAmountRangeRule(double Min, double Max) : IPendingActionPlanRollupRule
{
    public List<string> Check(PendingActionPlanRollup item)
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

/// <summary>Runs every rule of a pending action plan rollup.</summary>
public sealed record PendingActionPlanRollupValidator(List<IPendingActionPlanRollupRule> Rules)
{
    public List<string> Validate(PendingActionPlanRollup item)
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

    public void RequireValid(PendingActionPlanRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingActionPlanRollup> items)
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

/// <summary>Functions of pending action plan rollup rules.</summary>
public static class PendingActionPlanRollupRules
{
    public static PendingActionPlanRollupValidator DefaultPendingActionPlanRollupValidator()
    {
        return new PendingActionPlanRollupValidator(
            new List<IPendingActionPlanRollupRule>
            {
                new PendingActionPlanRollupRequiredBadgeRule("pending action plan rollup needs an entry"),
                new PendingActionPlanRollupRetriesRangeRule(1000, 3600),
                new PendingActionPlanRollupBadgeLengthRule(80),
                new PendingActionPlanRollupAllowedOwnerIdRule(new List<string>()),
                new PendingActionPlanRollupAmountRangeRule(1.0, 1530.8),
            });
    }
}
