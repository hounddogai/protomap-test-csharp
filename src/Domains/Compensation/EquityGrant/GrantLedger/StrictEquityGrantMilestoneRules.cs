namespace Acme.Workforce.Compensation.EquityGrant.GrantLedger;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a strict equity grant milestone must pass. Check returns its problems, or none.</summary>
public interface IStrictEquityGrantMilestoneRule
{
    List<string> Check(StrictEquityGrantMilestone item);
}

public sealed record StrictEquityGrantMilestoneAllowedTitleRule(List<string> Allowed) : IStrictEquityGrantMilestoneRule
{
    public List<string> Check(StrictEquityGrantMilestone item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is expired" };
        }
        return new List<string>();
    }
}

public sealed record StrictEquityGrantMilestoneRegionLengthRule(int MaxLength) : IStrictEquityGrantMilestoneRule
{
    public List<string> Check(StrictEquityGrantMilestone item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictEquityGrantMilestoneRequiredTitleRule(string Message) : IStrictEquityGrantMilestoneRule
{
    public List<string> Check(StrictEquityGrantMilestone item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictEquityGrantMilestoneDurationMinutesRangeRule(int Min, int Max) : IStrictEquityGrantMilestoneRule
{
    public List<string> Check(StrictEquityGrantMilestone item)
    {
        var valueValue = item.DurationMinutes;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"duration minutes {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StrictEquityGrantMilestoneTitleLengthRule(int MaxLength) : IStrictEquityGrantMilestoneRule
{
    public List<string> Check(StrictEquityGrantMilestone item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict equity grant milestone.</summary>
public sealed record StrictEquityGrantMilestoneValidator(List<IStrictEquityGrantMilestoneRule> Rules)
{
    public List<string> Validate(StrictEquityGrantMilestone item)
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

    public void RequireValid(StrictEquityGrantMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictEquityGrantMilestone> items)
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

/// <summary>Functions of strict equity grant milestone rules.</summary>
public static class StrictEquityGrantMilestoneRules
{
    public static StrictEquityGrantMilestoneValidator DefaultStrictEquityGrantMilestoneValidator()
    {
        return new StrictEquityGrantMilestoneValidator(
            new List<IStrictEquityGrantMilestoneRule>
            {
                new StrictEquityGrantMilestoneAllowedTitleRule(new List<string> { "scheduled" }),
                new StrictEquityGrantMilestoneRegionLengthRule(16),
                new StrictEquityGrantMilestoneRequiredTitleRule("strict equity grant milestone needs a value"),
                new StrictEquityGrantMilestoneDurationMinutesRangeRule(80, 1000),
                new StrictEquityGrantMilestoneTitleLengthRule(32),
            });
    }
}
