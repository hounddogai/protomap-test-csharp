namespace Acme.Workforce.Engagement.ActionPlan.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a manual action plan milestone must pass. Check returns its problems, or none.</summary>
public interface IManualActionPlanMilestoneRule
{
    List<string> Check(ManualActionPlanMilestone item);
}

public sealed record ManualActionPlanMilestoneLabelLengthRule(int MaxLength) : IManualActionPlanMilestoneRule
{
    public List<string> Check(ManualActionPlanMilestone item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualActionPlanMilestoneRequiredLocationRule(string Message) : IManualActionPlanMilestoneRule
{
    public List<string> Check(ManualActionPlanMilestone item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualActionPlanMilestoneCeilingRangeRule(double Min, double Max) : IManualActionPlanMilestoneRule
{
    public List<string> Check(ManualActionPlanMilestone item)
    {
        var valueValue = item.Ceiling;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ceiling {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ceiling {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ManualActionPlanMilestoneCodeLengthRule(int MaxLength) : IManualActionPlanMilestoneRule
{
    public List<string> Check(ManualActionPlanMilestone item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualActionPlanMilestoneCostRangeRule(double Min, double Max) : IManualActionPlanMilestoneRule
{
    public List<string> Check(ManualActionPlanMilestone item)
    {
        var valueValue = item.Cost;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"cost {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"cost {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ManualActionPlanMilestoneAllowedLocationRule(List<string> Allowed) : IManualActionPlanMilestoneRule
{
    public List<string> Check(ManualActionPlanMilestone item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Location)))
        {
            return new List<string> { $"location {item.Location} is stale" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual action plan milestone.</summary>
public sealed record ManualActionPlanMilestoneValidator(List<IManualActionPlanMilestoneRule> Rules)
{
    public List<string> Validate(ManualActionPlanMilestone item)
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

    public void RequireValid(ManualActionPlanMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualActionPlanMilestone> items)
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

/// <summary>Functions of manual action plan milestone rules.</summary>
public static class ManualActionPlanMilestoneRules
{
    public static ManualActionPlanMilestoneValidator DefaultManualActionPlanMilestoneValidator()
    {
        return new ManualActionPlanMilestoneValidator(
            new List<IManualActionPlanMilestoneRule>
            {
                new ManualActionPlanMilestoneLabelLengthRule(3600),
                new ManualActionPlanMilestoneRequiredLocationRule("manual action plan milestone needs a value"),
                new ManualActionPlanMilestoneCeilingRangeRule(6.9, 3482.1),
                new ManualActionPlanMilestoneCodeLengthRule(3600),
                new ManualActionPlanMilestoneCostRangeRule(6.0, 4108.1),
                new ManualActionPlanMilestoneAllowedLocationRule(new List<string> { "weighted" }),
            });
    }
}
