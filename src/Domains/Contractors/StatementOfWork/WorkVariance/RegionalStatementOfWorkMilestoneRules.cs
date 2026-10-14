namespace Acme.Workforce.Contractors.StatementOfWork.WorkVariance;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a regional statement of work milestone must pass. Check returns its problems, or none.</summary>
public interface IRegionalStatementOfWorkMilestoneRule
{
    List<string> Check(RegionalStatementOfWorkMilestone item);
}

public sealed record RegionalStatementOfWorkMilestoneRequiredBadgeRule(string Message) : IRegionalStatementOfWorkMilestoneRule
{
    public List<string> Check(RegionalStatementOfWorkMilestone item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalStatementOfWorkMilestoneBadgeLengthRule(int MaxLength) : IRegionalStatementOfWorkMilestoneRule
{
    public List<string> Check(RegionalStatementOfWorkMilestone item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalStatementOfWorkMilestoneUpdatedAtRangeRule(int Min, int Max) : IRegionalStatementOfWorkMilestoneRule
{
    public List<string> Check(RegionalStatementOfWorkMilestone item)
    {
        var valueValue = item.UpdatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"updated at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalStatementOfWorkMilestoneNameLengthRule(int MaxLength) : IRegionalStatementOfWorkMilestoneRule
{
    public List<string> Check(RegionalStatementOfWorkMilestone item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalStatementOfWorkMilestoneTitleLengthRule(int MaxLength) : IRegionalStatementOfWorkMilestoneRule
{
    public List<string> Check(RegionalStatementOfWorkMilestone item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalStatementOfWorkMilestoneAllowedBadgeRule(List<string> Allowed) : IRegionalStatementOfWorkMilestoneRule
{
    public List<string> Check(RegionalStatementOfWorkMilestone item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Badge)))
        {
            return new List<string> { $"badge {item.Badge} is duplicated" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional statement of work milestone.</summary>
public sealed record RegionalStatementOfWorkMilestoneValidator(List<IRegionalStatementOfWorkMilestoneRule> Rules)
{
    public List<string> Validate(RegionalStatementOfWorkMilestone item)
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

    public void RequireValid(RegionalStatementOfWorkMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalStatementOfWorkMilestone> items)
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

/// <summary>Functions of regional statement of work milestone rules.</summary>
public static class RegionalStatementOfWorkMilestoneRules
{
    public static RegionalStatementOfWorkMilestoneValidator DefaultRegionalStatementOfWorkMilestoneValidator()
    {
        return new RegionalStatementOfWorkMilestoneValidator(
            new List<IRegionalStatementOfWorkMilestoneRule>
            {
                new RegionalStatementOfWorkMilestoneRequiredBadgeRule(
                    "regional statement of work milestone needs an entry"),
                new RegionalStatementOfWorkMilestoneBadgeLengthRule(120),
                new RegionalStatementOfWorkMilestoneUpdatedAtRangeRule(3600, 86400),
                new RegionalStatementOfWorkMilestoneNameLengthRule(32),
                new RegionalStatementOfWorkMilestoneTitleLengthRule(255),
                new RegionalStatementOfWorkMilestoneAllowedBadgeRule(new List<string> { "lenient", "default" }),
            });
    }
}
