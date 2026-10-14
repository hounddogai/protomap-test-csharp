namespace Acme.Workforce.Payrollops.Garnishment.DefaultGarnishmentAllocation;

/// <summary>A rule a weighted garnishment milestone must pass. Check returns its problems, or none.</summary>
public interface IWeightedGarnishmentMilestoneRule
{
    List<string> Check(WeightedGarnishmentMilestone item);
}

public sealed record WeightedGarnishmentMilestoneCodePrefixRule(string Prefix) : IWeightedGarnishmentMilestoneRule
{
    public List<string> Check(WeightedGarnishmentMilestone item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedGarnishmentMilestoneRequiredCodeRule(string Message) : IWeightedGarnishmentMilestoneRule
{
    public List<string> Check(WeightedGarnishmentMilestone item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedGarnishmentMilestoneTitleLengthRule(int MaxLength) : IWeightedGarnishmentMilestoneRule
{
    public List<string> Check(WeightedGarnishmentMilestone item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedGarnishmentMilestoneSegmentLengthRule(int MaxLength) : IWeightedGarnishmentMilestoneRule
{
    public List<string> Check(WeightedGarnishmentMilestone item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted garnishment milestone.</summary>
public sealed record WeightedGarnishmentMilestoneValidator(List<IWeightedGarnishmentMilestoneRule> Rules)
{
    public List<string> Validate(WeightedGarnishmentMilestone item)
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

    public void RequireValid(WeightedGarnishmentMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedGarnishmentMilestone> items)
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

/// <summary>Functions of weighted garnishment milestone rules.</summary>
public static class WeightedGarnishmentMilestoneRules
{
    public static WeightedGarnishmentMilestoneValidator DefaultWeightedGarnishmentMilestoneValidator()
    {
        return new WeightedGarnishmentMilestoneValidator(
            new List<IWeightedGarnishmentMilestoneRule>
            {
                new WeightedGarnishmentMilestoneCodePrefixRule("REF"),
                new WeightedGarnishmentMilestoneRequiredCodeRule("weighted garnishment milestone needs an entry"),
                new WeightedGarnishmentMilestoneTitleLengthRule(120),
                new WeightedGarnishmentMilestoneSegmentLengthRule(3600),
            });
    }
}
