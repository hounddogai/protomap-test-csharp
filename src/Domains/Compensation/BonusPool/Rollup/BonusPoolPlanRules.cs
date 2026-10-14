namespace Acme.Workforce.Compensation.BonusPool.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a bonus pool plan must pass. Check returns its problems, or none.</summary>
public interface IBonusPoolPlanRule
{
    List<string> Check(BonusPoolPlan item);
}

public sealed record BonusPoolPlanRatingRangeRule(double Min, double Max) : IBonusPoolPlanRule
{
    public List<string> Check(BonusPoolPlan item)
    {
        var valueValue = item.Rating;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rating {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolPlanAllowedCodeRule(List<string> Allowed) : IBonusPoolPlanRule
{
    public List<string> Check(BonusPoolPlan item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is missing" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolPlanCodeLengthRule(int MaxLength) : IBonusPoolPlanRule
{
    public List<string> Check(BonusPoolPlan item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolPlanRatioRangeRule(double Min, double Max) : IBonusPoolPlanRule
{
    public List<string> Check(BonusPoolPlan item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolPlanRequiredNameRule(string Message) : IBonusPoolPlanRule
{
    public List<string> Check(BonusPoolPlan item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolPlanRequiredCodeRule(string Message) : IBonusPoolPlanRule
{
    public List<string> Check(BonusPoolPlan item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a bonus pool plan.</summary>
public sealed record BonusPoolPlanValidator(List<IBonusPoolPlanRule> Rules)
{
    public List<string> Validate(BonusPoolPlan item)
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

    public void RequireValid(BonusPoolPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BonusPoolPlan> items)
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

/// <summary>Functions of bonus pool plan rules.</summary>
public static class BonusPoolPlanRules
{
    public static BonusPoolPlanValidator DefaultBonusPoolPlanValidator()
    {
        return new BonusPoolPlanValidator(
            new List<IBonusPoolPlanRule>
            {
                new BonusPoolPlanRatingRangeRule(9.7, 3177.1),
                new BonusPoolPlanAllowedCodeRule(new List<string> { "tiered" }),
                new BonusPoolPlanCodeLengthRule(1000),
                new BonusPoolPlanRatioRangeRule(4.1, 3560.6),
                new BonusPoolPlanRequiredNameRule("bonus pool plan needs a value"),
                new BonusPoolPlanRequiredCodeRule("bonus pool plan needs this field"),
            });
    }
}
