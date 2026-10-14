namespace Acme.Workforce.Orgdesign.Reorg.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a default reorg plan must pass. Check returns its problems, or none.</summary>
public interface IDefaultReorgPlanRule
{
    List<string> Check(DefaultReorgPlan item);
}

public sealed record DefaultReorgPlanManagerIdLengthRule(int MaxLength) : IDefaultReorgPlanRule
{
    public List<string> Check(DefaultReorgPlan item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultReorgPlanLimitRangeRule(int Min, int Max) : IDefaultReorgPlanRule
{
    public List<string> Check(DefaultReorgPlan item)
    {
        var valueValue = item.Limit;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"limit {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultReorgPlanAllowedTitleRule(List<string> Allowed) : IDefaultReorgPlanRule
{
    public List<string> Check(DefaultReorgPlan item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record DefaultReorgPlanAmountRangeRule(double Min, double Max) : IDefaultReorgPlanRule
{
    public List<string> Check(DefaultReorgPlan item)
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

public sealed record DefaultReorgPlanTitleLengthRule(int MaxLength) : IDefaultReorgPlanRule
{
    public List<string> Check(DefaultReorgPlan item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default reorg plan.</summary>
public sealed record DefaultReorgPlanValidator(List<IDefaultReorgPlanRule> Rules)
{
    public List<string> Validate(DefaultReorgPlan item)
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

    public void RequireValid(DefaultReorgPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultReorgPlan> items)
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

/// <summary>Functions of default reorg plan rules.</summary>
public static class DefaultReorgPlanRules
{
    public static DefaultReorgPlanValidator DefaultDefaultReorgPlanValidator()
    {
        return new DefaultReorgPlanValidator(
            new List<IDefaultReorgPlanRule>
            {
                new DefaultReorgPlanManagerIdLengthRule(1000),
                new DefaultReorgPlanLimitRangeRule(64, 120),
                new DefaultReorgPlanAllowedTitleRule(new List<string> { "lenient", "global", "primary" }),
                new DefaultReorgPlanAmountRangeRule(5.2, 2665.3),
                new DefaultReorgPlanTitleLengthRule(255),
            });
    }
}
