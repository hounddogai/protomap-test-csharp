namespace Acme.Workforce.Absence.ReturnPlan.PlanAudit;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a strict return plan rollup must pass. Check returns its problems, or none.</summary>
public interface IStrictReturnPlanRollupRule
{
    List<string> Check(StrictReturnPlanRollup item);
}

public sealed record StrictReturnPlanRollupRequiredCurrencyRule(string Message) : IStrictReturnPlanRollupRule
{
    public List<string> Check(StrictReturnPlanRollup item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictReturnPlanRollupAllowedTimezoneRule(List<string> Allowed) : IStrictReturnPlanRollupRule
{
    public List<string> Check(StrictReturnPlanRollup item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Timezone)))
        {
            return new List<string> { $"timezone {item.Timezone} is stale" };
        }
        return new List<string>();
    }
}

public sealed record StrictReturnPlanRollupRequiredCodeRule(string Message) : IStrictReturnPlanRollupRule
{
    public List<string> Check(StrictReturnPlanRollup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictReturnPlanRollupRequiredTitleRule(string Message) : IStrictReturnPlanRollupRule
{
    public List<string> Check(StrictReturnPlanRollup item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictReturnPlanRollupThresholdRangeRule(double Min, double Max) : IStrictReturnPlanRollupRule
{
    public List<string> Check(StrictReturnPlanRollup item)
    {
        var valueValue = item.Threshold;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"threshold {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"threshold {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record StrictReturnPlanRollupMarginRangeRule(double Min, double Max) : IStrictReturnPlanRollupRule
{
    public List<string> Check(StrictReturnPlanRollup item)
    {
        var valueValue = item.Margin;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"margin {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict return plan rollup.</summary>
public sealed record StrictReturnPlanRollupValidator(List<IStrictReturnPlanRollupRule> Rules)
{
    public List<string> Validate(StrictReturnPlanRollup item)
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

    public void RequireValid(StrictReturnPlanRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictReturnPlanRollup> items)
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

/// <summary>Functions of strict return plan rollup rules.</summary>
public static class StrictReturnPlanRollupRules
{
    public static StrictReturnPlanRollupValidator DefaultStrictReturnPlanRollupValidator()
    {
        return new StrictReturnPlanRollupValidator(
            new List<IStrictReturnPlanRollupRule>
            {
                new StrictReturnPlanRollupRequiredCurrencyRule("strict return plan rollup needs a value"),
                new StrictReturnPlanRollupAllowedTimezoneRule(new List<string> { "manual" }),
                new StrictReturnPlanRollupRequiredCodeRule("strict return plan rollup needs an entry"),
                new StrictReturnPlanRollupRequiredTitleRule("strict return plan rollup needs a value"),
                new StrictReturnPlanRollupThresholdRangeRule(5.6, 400.8),
                new StrictReturnPlanRollupMarginRangeRule(4.7, 4488.3),
            });
    }
}
