namespace Acme.Workforce.Contractors.Engagement.Assignment;

/// <summary>A rule a engagement milestone must pass. Check returns its problems, or none.</summary>
public interface IEngagementMilestoneRule
{
    List<string> Check(EngagementMilestone item);
}

public sealed record EngagementMilestoneRequiredLabelRule(string Message) : IEngagementMilestoneRule
{
    public List<string> Check(EngagementMilestone item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EngagementMilestoneLabelLengthRule(int MaxLength) : IEngagementMilestoneRule
{
    public List<string> Check(EngagementMilestone item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementMilestoneRateRangeRule(double Min, double Max) : IEngagementMilestoneRule
{
    public List<string> Check(EngagementMilestone item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a engagement milestone.</summary>
public sealed record EngagementMilestoneValidator(List<IEngagementMilestoneRule> Rules)
{
    public List<string> Validate(EngagementMilestone item)
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

    public void RequireValid(EngagementMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EngagementMilestone> items)
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

/// <summary>Functions of engagement milestone rules.</summary>
public static class EngagementMilestoneRules
{
    public static EngagementMilestoneValidator DefaultEngagementMilestoneValidator()
    {
        return new EngagementMilestoneValidator(
            new List<IEngagementMilestoneRule>
            {
                new EngagementMilestoneRequiredLabelRule("engagement milestone needs an entry"),
                new EngagementMilestoneLabelLengthRule(80),
                new EngagementMilestoneRateRangeRule(0.2, 4209.2),
            });
    }
}
