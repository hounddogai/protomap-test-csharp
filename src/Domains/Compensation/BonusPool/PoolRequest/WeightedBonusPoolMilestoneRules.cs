namespace Acme.Workforce.Compensation.BonusPool.PoolRequest;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a weighted bonus pool milestone must pass. Check returns its problems, or none.</summary>
public interface IWeightedBonusPoolMilestoneRule
{
    List<string> Check(WeightedBonusPoolMilestone item);
}

public sealed record WeightedBonusPoolMilestoneCodeLengthRule(int MaxLength) : IWeightedBonusPoolMilestoneRule
{
    public List<string> Check(WeightedBonusPoolMilestone item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedBonusPoolMilestoneAllowedLabelRule(List<string> Allowed) : IWeightedBonusPoolMilestoneRule
{
    public List<string> Check(WeightedBonusPoolMilestone item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record WeightedBonusPoolMilestoneSummaryLengthRule(int MaxLength) : IWeightedBonusPoolMilestoneRule
{
    public List<string> Check(WeightedBonusPoolMilestone item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted bonus pool milestone.</summary>
public sealed record WeightedBonusPoolMilestoneValidator(List<IWeightedBonusPoolMilestoneRule> Rules)
{
    public List<string> Validate(WeightedBonusPoolMilestone item)
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

    public void RequireValid(WeightedBonusPoolMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedBonusPoolMilestone> items)
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

/// <summary>Functions of weighted bonus pool milestone rules.</summary>
public static class WeightedBonusPoolMilestoneRules
{
    public static WeightedBonusPoolMilestoneValidator DefaultWeightedBonusPoolMilestoneValidator()
    {
        return new WeightedBonusPoolMilestoneValidator(
            new List<IWeightedBonusPoolMilestoneRule>
            {
                new WeightedBonusPoolMilestoneCodeLengthRule(120),
                new WeightedBonusPoolMilestoneAllowedLabelRule(new List<string> { "default", "pending", "global" }),
                new WeightedBonusPoolMilestoneSummaryLengthRule(86400),
            });
    }
}
