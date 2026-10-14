namespace Acme.Workforce.Contractors.Rate.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a capped rate milestone must pass. Check returns its problems, or none.</summary>
public interface ICappedRateMilestoneRule
{
    List<string> Check(CappedRateMilestone item);
}

public sealed record CappedRateMilestoneGraceDaysRangeRule(int Min, int Max) : ICappedRateMilestoneRule
{
    public List<string> Check(CappedRateMilestone item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CappedRateMilestoneRequiredLocationRule(string Message) : ICappedRateMilestoneRule
{
    public List<string> Check(CappedRateMilestone item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedRateMilestoneLimitRangeRule(int Min, int Max) : ICappedRateMilestoneRule
{
    public List<string> Check(CappedRateMilestone item)
    {
        var valueValue = item.Limit;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"limit {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CappedRateMilestoneAllowedCodeRule(List<string> Allowed) : ICappedRateMilestoneRule
{
    public List<string> Check(CappedRateMilestone item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record CappedRateMilestoneCodePrefixRule(string Prefix) : ICappedRateMilestoneRule
{
    public List<string> Check(CappedRateMilestone item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped rate milestone.</summary>
public sealed record CappedRateMilestoneValidator(List<ICappedRateMilestoneRule> Rules)
{
    public List<string> Validate(CappedRateMilestone item)
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

    public void RequireValid(CappedRateMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedRateMilestone> items)
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

/// <summary>Functions of capped rate milestone rules.</summary>
public static class CappedRateMilestoneRules
{
    public static CappedRateMilestoneValidator DefaultCappedRateMilestoneValidator()
    {
        return new CappedRateMilestoneValidator(
            new List<ICappedRateMilestoneRule>
            {
                new CappedRateMilestoneGraceDaysRangeRule(64, 80),
                new CappedRateMilestoneRequiredLocationRule("capped rate milestone needs this field"),
                new CappedRateMilestoneLimitRangeRule(64, 120),
                new CappedRateMilestoneAllowedCodeRule(new List<string> { "lenient", "batched", "global" }),
                new CappedRateMilestoneCodePrefixRule("OPS"),
            });
    }
}
