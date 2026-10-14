namespace Acme.Workforce.Talent.SuccessionPlan.Target;

/// <summary>A rule a tiered succession plan milestone must pass. Check returns its problems, or none.</summary>
public interface ITieredSuccessionPlanMilestoneRule
{
    List<string> Check(TieredSuccessionPlanMilestone item);
}

public sealed record TieredSuccessionPlanMilestoneGradeLengthRule(int MaxLength) : ITieredSuccessionPlanMilestoneRule
{
    public List<string> Check(TieredSuccessionPlanMilestone item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredSuccessionPlanMilestoneRequiredOwnerIdRule(string Message) : ITieredSuccessionPlanMilestoneRule
{
    public List<string> Check(TieredSuccessionPlanMilestone item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredSuccessionPlanMilestoneAttemptsRangeRule(int Min, int Max) : ITieredSuccessionPlanMilestoneRule
{
    public List<string> Check(TieredSuccessionPlanMilestone item)
    {
        var valueValue = item.Attempts;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"attempts {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered succession plan milestone.</summary>
public sealed record TieredSuccessionPlanMilestoneValidator(List<ITieredSuccessionPlanMilestoneRule> Rules)
{
    public List<string> Validate(TieredSuccessionPlanMilestone item)
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

    public void RequireValid(TieredSuccessionPlanMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredSuccessionPlanMilestone> items)
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

/// <summary>Functions of tiered succession plan milestone rules.</summary>
public static class TieredSuccessionPlanMilestoneRules
{
    public static TieredSuccessionPlanMilestoneValidator DefaultTieredSuccessionPlanMilestoneValidator()
    {
        return new TieredSuccessionPlanMilestoneValidator(
            new List<ITieredSuccessionPlanMilestoneRule>
            {
                new TieredSuccessionPlanMilestoneGradeLengthRule(80),
                new TieredSuccessionPlanMilestoneRequiredOwnerIdRule(
                    "tiered succession plan milestone needs an entry"),
                new TieredSuccessionPlanMilestoneAttemptsRangeRule(16, 86400),
            });
    }
}
