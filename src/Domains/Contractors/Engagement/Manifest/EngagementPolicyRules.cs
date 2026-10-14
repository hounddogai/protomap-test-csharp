namespace Acme.Workforce.Contractors.Engagement.Manifest;

/// <summary>A rule a engagement policy must pass. Check returns its problems, or none.</summary>
public interface IEngagementPolicyRule
{
    List<string> Check(EngagementPolicy item);
}

public sealed record EngagementPolicyMinimumRangeRule(double Min, double Max) : IEngagementPolicyRule
{
    public List<string> Check(EngagementPolicy item)
    {
        var valueValue = item.Minimum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"minimum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"minimum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record EngagementPolicySequenceRangeRule(int Min, int Max) : IEngagementPolicyRule
{
    public List<string> Check(EngagementPolicy item)
    {
        var valueValue = item.Sequence;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"sequence {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementPolicyUpdatedAtRangeRule(int Min, int Max) : IEngagementPolicyRule
{
    public List<string> Check(EngagementPolicy item)
    {
        var valueValue = item.UpdatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"updated at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementPolicyRequiredTeamRule(string Message) : IEngagementPolicyRule
{
    public List<string> Check(EngagementPolicy item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EngagementPolicyRequiredTitleRule(string Message) : IEngagementPolicyRule
{
    public List<string> Check(EngagementPolicy item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a engagement policy.</summary>
public sealed record EngagementPolicyValidator(List<IEngagementPolicyRule> Rules)
{
    public List<string> Validate(EngagementPolicy item)
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

    public void RequireValid(EngagementPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EngagementPolicy> items)
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

/// <summary>Functions of engagement policy rules.</summary>
public static class EngagementPolicyRules
{
    public static EngagementPolicyValidator DefaultEngagementPolicyValidator()
    {
        return new EngagementPolicyValidator(
            new List<IEngagementPolicyRule>
            {
                new EngagementPolicyMinimumRangeRule(9.6, 2917.4),
                new EngagementPolicySequenceRangeRule(32, 86400),
                new EngagementPolicyUpdatedAtRangeRule(86400, 86400),
                new EngagementPolicyRequiredTeamRule("engagement policy needs this field"),
                new EngagementPolicyRequiredTitleRule("engagement policy needs this field"),
            });
    }
}
