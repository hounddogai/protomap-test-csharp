namespace Acme.Workforce.Engagement.ActionPlan.Claim;

/// <summary>A rule a default action plan rollup must pass. Check returns its problems, or none.</summary>
public interface IDefaultActionPlanRollupRule
{
    List<string> Check(DefaultActionPlanRollup item);
}

public sealed record DefaultActionPlanRollupRequiredTeamRule(string Message) : IDefaultActionPlanRollupRule
{
    public List<string> Check(DefaultActionPlanRollup item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultActionPlanRollupTeamLengthRule(int MaxLength) : IDefaultActionPlanRollupRule
{
    public List<string> Check(DefaultActionPlanRollup item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultActionPlanRollupStartsAtRangeRule(int Min, int Max) : IDefaultActionPlanRollupRule
{
    public List<string> Check(DefaultActionPlanRollup item)
    {
        var valueValue = item.StartsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"starts at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"starts at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DefaultActionPlanRollupRequiredCodeRule(string Message) : IDefaultActionPlanRollupRule
{
    public List<string> Check(DefaultActionPlanRollup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default action plan rollup.</summary>
public sealed record DefaultActionPlanRollupValidator(List<IDefaultActionPlanRollupRule> Rules)
{
    public List<string> Validate(DefaultActionPlanRollup item)
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

    public void RequireValid(DefaultActionPlanRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultActionPlanRollup> items)
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

/// <summary>Functions of default action plan rollup rules.</summary>
public static class DefaultActionPlanRollupRules
{
    public static DefaultActionPlanRollupValidator DefaultDefaultActionPlanRollupValidator()
    {
        return new DefaultActionPlanRollupValidator(
            new List<IDefaultActionPlanRollupRule>
            {
                new DefaultActionPlanRollupRequiredTeamRule("default action plan rollup needs this field"),
                new DefaultActionPlanRollupTeamLengthRule(16),
                new DefaultActionPlanRollupStartsAtRangeRule(80, 255),
                new DefaultActionPlanRollupRequiredCodeRule("default action plan rollup needs a value"),
            });
    }
}
