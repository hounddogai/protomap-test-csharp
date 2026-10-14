namespace Acme.Workforce.Payrollops.RetroPay.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a seasonal retro pay review must pass. Check returns its problems, or none.</summary>
public interface ISeasonalRetroPayReviewRule
{
    List<string> Check(SeasonalRetroPayReview item);
}

public sealed record SeasonalRetroPayReviewLeaveDaysRangeRule(int Min, int Max) : ISeasonalRetroPayReviewRule
{
    public List<string> Check(SeasonalRetroPayReview item)
    {
        var valueValue = item.LeaveDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"leave days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"leave days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SeasonalRetroPayReviewCodeLengthRule(int MaxLength) : ISeasonalRetroPayReviewRule
{
    public List<string> Check(SeasonalRetroPayReview item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalRetroPayReviewAllowedCategoryRule(List<string> Allowed) : ISeasonalRetroPayReviewRule
{
    public List<string> Check(SeasonalRetroPayReview item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Category)))
        {
            return new List<string> { $"category {item.Category} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalRetroPayReviewSourceLengthRule(int MaxLength) : ISeasonalRetroPayReviewRule
{
    public List<string> Check(SeasonalRetroPayReview item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalRetroPayReviewPhoneLengthRule(int MaxLength) : ISeasonalRetroPayReviewRule
{
    public List<string> Check(SeasonalRetroPayReview item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal retro pay review.</summary>
public sealed record SeasonalRetroPayReviewValidator(List<ISeasonalRetroPayReviewRule> Rules)
{
    public List<string> Validate(SeasonalRetroPayReview item)
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

    public void RequireValid(SeasonalRetroPayReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalRetroPayReview> items)
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

/// <summary>Functions of seasonal retro pay review rules.</summary>
public static class SeasonalRetroPayReviewRules
{
    public static SeasonalRetroPayReviewValidator DefaultSeasonalRetroPayReviewValidator()
    {
        return new SeasonalRetroPayReviewValidator(
            new List<ISeasonalRetroPayReviewRule>
            {
                new SeasonalRetroPayReviewLeaveDaysRangeRule(16, 255),
                new SeasonalRetroPayReviewCodeLengthRule(16),
                new SeasonalRetroPayReviewAllowedCategoryRule(new List<string> { "regional" }),
                new SeasonalRetroPayReviewSourceLengthRule(16),
                new SeasonalRetroPayReviewPhoneLengthRule(3600),
            });
    }
}
