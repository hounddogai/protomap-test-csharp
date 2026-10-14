namespace Acme.Workforce.Talent.Pool.Plan;

/// <summary>A rule a scheduled pool plan must pass. Check returns its problems, or none.</summary>
public interface IScheduledPoolPlanRule
{
    List<string> Check(ScheduledPoolPlan item);
}

public sealed record ScheduledPoolPlanRequiredCurrencyRule(string Message) : IScheduledPoolPlanRule
{
    public List<string> Check(ScheduledPoolPlan item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPoolPlanTimezoneLengthRule(int MaxLength) : IScheduledPoolPlanRule
{
    public List<string> Check(ScheduledPoolPlan item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPoolPlanCodePrefixRule(string Prefix) : IScheduledPoolPlanRule
{
    public List<string> Check(ScheduledPoolPlan item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled pool plan.</summary>
public sealed record ScheduledPoolPlanValidator(List<IScheduledPoolPlanRule> Rules)
{
    public List<string> Validate(ScheduledPoolPlan item)
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

    public void RequireValid(ScheduledPoolPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledPoolPlan> items)
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

/// <summary>Functions of scheduled pool plan rules.</summary>
public static class ScheduledPoolPlanRules
{
    public static ScheduledPoolPlanValidator DefaultScheduledPoolPlanValidator()
    {
        return new ScheduledPoolPlanValidator(
            new List<IScheduledPoolPlanRule>
            {
                new ScheduledPoolPlanRequiredCurrencyRule("scheduled pool plan needs a value"),
                new ScheduledPoolPlanTimezoneLengthRule(120),
                new ScheduledPoolPlanCodePrefixRule("X"),
            });
    }
}
