namespace Acme.Workforce.Talent.Nomination.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a weighted nomination milestone must pass. Check returns its problems, or none.</summary>
public interface IWeightedNominationMilestoneRule
{
    List<string> Check(WeightedNominationMilestone item);
}

public sealed record WeightedNominationMilestoneRequiredCodeRule(string Message) : IWeightedNominationMilestoneRule
{
    public List<string> Check(WeightedNominationMilestone item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedNominationMilestoneGraceDaysRangeRule(int Min, int Max) : IWeightedNominationMilestoneRule
{
    public List<string> Check(WeightedNominationMilestone item)
    {
        var valueValue = item.GraceDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"grace days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"grace days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record WeightedNominationMilestoneCodePrefixRule(string Prefix) : IWeightedNominationMilestoneRule
{
    public List<string> Check(WeightedNominationMilestone item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedNominationMilestoneAllowedCategoryRule(List<string> Allowed) : IWeightedNominationMilestoneRule
{
    public List<string> Check(WeightedNominationMilestone item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Category)))
        {
            return new List<string> { $"category {item.Category} is stale" };
        }
        return new List<string>();
    }
}

public sealed record WeightedNominationMilestoneStartsAtRangeRule(int Min, int Max) : IWeightedNominationMilestoneRule
{
    public List<string> Check(WeightedNominationMilestone item)
    {
        var valueValue = item.StartsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"starts at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"starts at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record WeightedNominationMilestoneRequiredNameRule(string Message) : IWeightedNominationMilestoneRule
{
    public List<string> Check(WeightedNominationMilestone item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted nomination milestone.</summary>
public sealed record WeightedNominationMilestoneValidator(List<IWeightedNominationMilestoneRule> Rules)
{
    public List<string> Validate(WeightedNominationMilestone item)
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

    public void RequireValid(WeightedNominationMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedNominationMilestone> items)
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

/// <summary>Functions of weighted nomination milestone rules.</summary>
public static class WeightedNominationMilestoneRules
{
    public static WeightedNominationMilestoneValidator DefaultWeightedNominationMilestoneValidator()
    {
        return new WeightedNominationMilestoneValidator(
            new List<IWeightedNominationMilestoneRule>
            {
                new WeightedNominationMilestoneRequiredCodeRule("weighted nomination milestone needs this field"),
                new WeightedNominationMilestoneGraceDaysRangeRule(16, 32),
                new WeightedNominationMilestoneCodePrefixRule("ACM"),
                new WeightedNominationMilestoneAllowedCategoryRule(new List<string>()),
                new WeightedNominationMilestoneStartsAtRangeRule(64, 120),
                new WeightedNominationMilestoneRequiredNameRule("weighted nomination milestone needs an entry"),
            });
    }
}
