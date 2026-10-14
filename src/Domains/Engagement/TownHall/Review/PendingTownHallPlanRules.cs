namespace Acme.Workforce.Engagement.TownHall.Review;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pending town hall plan must pass. Check returns its problems, or none.</summary>
public interface IPendingTownHallPlanRule
{
    List<string> Check(PendingTownHallPlan item);
}

public sealed record PendingTownHallPlanAllowedTitleRule(List<string> Allowed) : IPendingTownHallPlanRule
{
    public List<string> Check(PendingTownHallPlan item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record PendingTownHallPlanCodeLengthRule(int MaxLength) : IPendingTownHallPlanRule
{
    public List<string> Check(PendingTownHallPlan item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingTownHallPlanCodePrefixRule(string Prefix) : IPendingTownHallPlanRule
{
    public List<string> Check(PendingTownHallPlan item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending town hall plan.</summary>
public sealed record PendingTownHallPlanValidator(List<IPendingTownHallPlanRule> Rules)
{
    public List<string> Validate(PendingTownHallPlan item)
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

    public void RequireValid(PendingTownHallPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingTownHallPlan> items)
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

/// <summary>Functions of pending town hall plan rules.</summary>
public static class PendingTownHallPlanRules
{
    public static PendingTownHallPlanValidator DefaultPendingTownHallPlanValidator()
    {
        return new PendingTownHallPlanValidator(
            new List<IPendingTownHallPlanRule>
            {
                new PendingTownHallPlanAllowedTitleRule(new List<string> { "regional", "strict", "manual" }),
                new PendingTownHallPlanCodeLengthRule(3600),
                new PendingTownHallPlanCodePrefixRule("Q"),
            });
    }
}
