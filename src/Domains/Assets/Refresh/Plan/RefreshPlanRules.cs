namespace Acme.Workforce.Assets.Refresh.Plan;

/// <summary>A rule a refresh plan must pass. Check returns its problems, or none.</summary>
public interface IRefreshPlanRule
{
    List<string> Check(RefreshPlan item);
}

public sealed record RefreshPlanRequiredLabelRule(string Message) : IRefreshPlanRule
{
    public List<string> Check(RefreshPlan item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RefreshPlanAttemptsRangeRule(int Min, int Max) : IRefreshPlanRule
{
    public List<string> Check(RefreshPlan item)
    {
        var valueValue = item.Attempts;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"attempts {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"attempts {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RefreshPlanCodePrefixRule(string Prefix) : IRefreshPlanRule
{
    public List<string> Check(RefreshPlan item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshPlanRequiredSegmentRule(string Message) : IRefreshPlanRule
{
    public List<string> Check(RefreshPlan item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RefreshPlanThresholdRangeRule(double Min, double Max) : IRefreshPlanRule
{
    public List<string> Check(RefreshPlan item)
    {
        var valueValue = item.Threshold;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"threshold {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a refresh plan.</summary>
public sealed record RefreshPlanValidator(List<IRefreshPlanRule> Rules)
{
    public List<string> Validate(RefreshPlan item)
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

    public void RequireValid(RefreshPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RefreshPlan> items)
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

/// <summary>Functions of refresh plan rules.</summary>
public static class RefreshPlanRules
{
    public static RefreshPlanValidator DefaultRefreshPlanValidator()
    {
        return new RefreshPlanValidator(
            new List<IRefreshPlanRule>
            {
                new RefreshPlanRequiredLabelRule("refresh plan needs an entry"),
                new RefreshPlanAttemptsRangeRule(64, 86400),
                new RefreshPlanCodePrefixRule("X"),
                new RefreshPlanRequiredSegmentRule("refresh plan needs this field"),
                new RefreshPlanThresholdRangeRule(8.8, 4913.8),
            });
    }
}
