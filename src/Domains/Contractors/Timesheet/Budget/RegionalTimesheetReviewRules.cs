namespace Acme.Workforce.Contractors.Timesheet.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a regional timesheet review must pass. Check returns its problems, or none.</summary>
public interface IRegionalTimesheetReviewRule
{
    List<string> Check(RegionalTimesheetReview item);
}

public sealed record RegionalTimesheetReviewRequiredTeamRule(string Message) : IRegionalTimesheetReviewRule
{
    public List<string> Check(RegionalTimesheetReview item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalTimesheetReviewAllowedLocationRule(List<string> Allowed) : IRegionalTimesheetReviewRule
{
    public List<string> Check(RegionalTimesheetReview item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Location)))
        {
            return new List<string> { $"location {item.Location} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record RegionalTimesheetReviewBadgeLengthRule(int MaxLength) : IRegionalTimesheetReviewRule
{
    public List<string> Check(RegionalTimesheetReview item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalTimesheetReviewTeamLengthRule(int MaxLength) : IRegionalTimesheetReviewRule
{
    public List<string> Check(RegionalTimesheetReview item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalTimesheetReviewAttemptsRangeRule(int Min, int Max) : IRegionalTimesheetReviewRule
{
    public List<string> Check(RegionalTimesheetReview item)
    {
        var valueValue = item.Attempts;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"attempts {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional timesheet review.</summary>
public sealed record RegionalTimesheetReviewValidator(List<IRegionalTimesheetReviewRule> Rules)
{
    public List<string> Validate(RegionalTimesheetReview item)
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

    public void RequireValid(RegionalTimesheetReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalTimesheetReview> items)
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

/// <summary>Functions of regional timesheet review rules.</summary>
public static class RegionalTimesheetReviewRules
{
    public static RegionalTimesheetReviewValidator DefaultRegionalTimesheetReviewValidator()
    {
        return new RegionalTimesheetReviewValidator(
            new List<IRegionalTimesheetReviewRule>
            {
                new RegionalTimesheetReviewRequiredTeamRule("regional timesheet review needs an entry"),
                new RegionalTimesheetReviewAllowedLocationRule(new List<string> { "regional", "weighted" }),
                new RegionalTimesheetReviewBadgeLengthRule(80),
                new RegionalTimesheetReviewTeamLengthRule(1000),
                new RegionalTimesheetReviewAttemptsRangeRule(16, 64),
            });
    }
}
