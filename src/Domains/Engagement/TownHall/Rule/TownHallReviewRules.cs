namespace Acme.Workforce.Engagement.TownHall.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a town hall review must pass. Check returns its problems, or none.</summary>
public interface ITownHallReviewRule
{
    List<string> Check(TownHallReview item);
}

public sealed record TownHallReviewAllowedTitleRule(List<string> Allowed) : ITownHallReviewRule
{
    public List<string> Check(TownHallReview item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is missing" };
        }
        return new List<string>();
    }
}

public sealed record TownHallReviewPriorityRangeRule(int Min, int Max) : ITownHallReviewRule
{
    public List<string> Check(TownHallReview item)
    {
        var valueValue = item.Priority;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"priority {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"priority {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TownHallReviewLeaveDaysRangeRule(int Min, int Max) : ITownHallReviewRule
{
    public List<string> Check(TownHallReview item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a town hall review.</summary>
public sealed record TownHallReviewValidator(List<ITownHallReviewRule> Rules)
{
    public List<string> Validate(TownHallReview item)
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

    public void RequireValid(TownHallReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TownHallReview> items)
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

/// <summary>Functions of town hall review rules.</summary>
public static class TownHallReviewRules
{
    public static TownHallReviewValidator DefaultTownHallReviewValidator()
    {
        return new TownHallReviewValidator(
            new List<ITownHallReviewRule>
            {
                new TownHallReviewAllowedTitleRule(new List<string> { "seasonal", "default", "pending" }),
                new TownHallReviewPriorityRangeRule(255, 86400),
                new TownHallReviewLeaveDaysRangeRule(255, 86400),
            });
    }
}
