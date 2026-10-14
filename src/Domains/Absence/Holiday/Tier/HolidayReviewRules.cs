namespace Acme.Workforce.Absence.Holiday.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a holiday review must pass. Check returns its problems, or none.</summary>
public interface IHolidayReviewRule
{
    List<string> Check(HolidayReview item);
}

public sealed record HolidayReviewScoreRangeRule(double Min, double Max) : IHolidayReviewRule
{
    public List<string> Check(HolidayReview item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record HolidayReviewTitleLengthRule(int MaxLength) : IHolidayReviewRule
{
    public List<string> Check(HolidayReview item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HolidayReviewActiveGuardRule(int Limit) : IHolidayReviewRule
{
    public List<string> Check(HolidayReview item)
    {
        if (item.Active && item.StartsAt > Limit)
        {
            return new List<string> { $"starts at {item.StartsAt} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record HolidayReviewAllowedJobCodeRule(List<string> Allowed) : IHolidayReviewRule
{
    public List<string> Check(HolidayReview item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.JobCode)))
        {
            return new List<string> { $"job code {item.JobCode} is above maximum" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a holiday review.</summary>
public sealed record HolidayReviewValidator(List<IHolidayReviewRule> Rules)
{
    public List<string> Validate(HolidayReview item)
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

    public void RequireValid(HolidayReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HolidayReview> items)
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

/// <summary>Functions of holiday review rules.</summary>
public static class HolidayReviewRules
{
    public static HolidayReviewValidator DefaultHolidayReviewValidator()
    {
        return new HolidayReviewValidator(
            new List<IHolidayReviewRule>
            {
                new HolidayReviewScoreRangeRule(7.0, 4202.6),
                new HolidayReviewTitleLengthRule(255),
                new HolidayReviewActiveGuardRule(16),
                new HolidayReviewAllowedJobCodeRule(new List<string> { "regional", "legacy", "manual" }),
            });
    }
}
