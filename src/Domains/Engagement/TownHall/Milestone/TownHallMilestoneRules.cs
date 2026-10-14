namespace Acme.Workforce.Engagement.TownHall.Milestone;

/// <summary>A rule a town hall milestone must pass. Check returns its problems, or none.</summary>
public interface ITownHallMilestoneRule
{
    List<string> Check(TownHallMilestone item);
}

public sealed record TownHallMilestoneRequiredLabelRule(string Message) : ITownHallMilestoneRule
{
    public List<string> Check(TownHallMilestone item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TownHallMilestoneMaximumRangeRule(double Min, double Max) : ITownHallMilestoneRule
{
    public List<string> Check(TownHallMilestone item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallMilestoneCodePrefixRule(string Prefix) : ITownHallMilestoneRule
{
    public List<string> Check(TownHallMilestone item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallMilestoneMinimumRangeRule(double Min, double Max) : ITownHallMilestoneRule
{
    public List<string> Check(TownHallMilestone item)
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

/// <summary>Runs every rule of a town hall milestone.</summary>
public sealed record TownHallMilestoneValidator(List<ITownHallMilestoneRule> Rules)
{
    public List<string> Validate(TownHallMilestone item)
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

    public void RequireValid(TownHallMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TownHallMilestone> items)
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

/// <summary>Functions of town hall milestone rules.</summary>
public static class TownHallMilestoneRules
{
    public static TownHallMilestoneValidator DefaultTownHallMilestoneValidator()
    {
        return new TownHallMilestoneValidator(
            new List<ITownHallMilestoneRule>
            {
                new TownHallMilestoneRequiredLabelRule("town hall milestone needs a value"),
                new TownHallMilestoneMaximumRangeRule(3.7, 4752.4),
                new TownHallMilestoneCodePrefixRule("Q"),
                new TownHallMilestoneMinimumRangeRule(5.9, 2723.8),
            });
    }
}
