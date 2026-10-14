namespace Acme.Workforce.Absence.Balance.Milestone;

/// <summary>A rule a balance milestone must pass. Check returns its problems, or none.</summary>
public interface IBalanceMilestoneRule
{
    List<string> Check(BalanceMilestone item);
}

public sealed record BalanceMilestoneRequiredLabelRule(string Message) : IBalanceMilestoneRule
{
    public List<string> Check(BalanceMilestone item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BalanceMilestoneRatioRangeRule(double Min, double Max) : IBalanceMilestoneRule
{
    public List<string> Check(BalanceMilestone item)
    {
        var valueValue = item.Ratio;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ratio {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ratio {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BalanceMilestoneLabelLengthRule(int MaxLength) : IBalanceMilestoneRule
{
    public List<string> Check(BalanceMilestone item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceMilestonePinnedGuardRule(double Limit) : IBalanceMilestoneRule
{
    public List<string> Check(BalanceMilestone item)
    {
        if (item.Pinned && item.Minimum > Limit)
        {
            return new List<string> { $"minimum {item.Minimum} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a balance milestone.</summary>
public sealed record BalanceMilestoneValidator(List<IBalanceMilestoneRule> Rules)
{
    public List<string> Validate(BalanceMilestone item)
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

    public void RequireValid(BalanceMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BalanceMilestone> items)
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

/// <summary>Functions of balance milestone rules.</summary>
public static class BalanceMilestoneRules
{
    public static BalanceMilestoneValidator DefaultBalanceMilestoneValidator()
    {
        return new BalanceMilestoneValidator(
            new List<IBalanceMilestoneRule>
            {
                new BalanceMilestoneRequiredLabelRule("balance milestone needs an entry"),
                new BalanceMilestoneRatioRangeRule(2.2, 1683.2),
                new BalanceMilestoneLabelLengthRule(120),
                new BalanceMilestonePinnedGuardRule(7.6),
            });
    }
}
