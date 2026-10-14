namespace Acme.Workforce.Compensation.Adjustment.Profile;

/// <summary>A rule a seasonal adjustment review must pass. Check returns its problems, or none.</summary>
public interface ISeasonalAdjustmentReviewRule
{
    List<string> Check(SeasonalAdjustmentReview item);
}

public sealed record SeasonalAdjustmentReviewUpdatedAtRangeRule(int Min, int Max) : ISeasonalAdjustmentReviewRule
{
    public List<string> Check(SeasonalAdjustmentReview item)
    {
        var valueValue = item.UpdatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"updated at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"updated at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SeasonalAdjustmentReviewCodePrefixRule(string Prefix) : ISeasonalAdjustmentReviewRule
{
    public List<string> Check(SeasonalAdjustmentReview item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalAdjustmentReviewTagsLimitRule(int MaxCount, List<string> Banned) : ISeasonalAdjustmentReviewRule
{
    public List<string> Check(SeasonalAdjustmentReview item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

public sealed record SeasonalAdjustmentReviewTitleLengthRule(int MaxLength) : ISeasonalAdjustmentReviewRule
{
    public List<string> Check(SeasonalAdjustmentReview item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalAdjustmentReviewRequiredNotesRule(string Message) : ISeasonalAdjustmentReviewRule
{
    public List<string> Check(SeasonalAdjustmentReview item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalAdjustmentReviewCodeLengthRule(int MaxLength) : ISeasonalAdjustmentReviewRule
{
    public List<string> Check(SeasonalAdjustmentReview item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal adjustment review.</summary>
public sealed record SeasonalAdjustmentReviewValidator(List<ISeasonalAdjustmentReviewRule> Rules)
{
    public List<string> Validate(SeasonalAdjustmentReview item)
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

    public void RequireValid(SeasonalAdjustmentReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalAdjustmentReview> items)
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

/// <summary>Functions of seasonal adjustment review rules.</summary>
public static class SeasonalAdjustmentReviewRules
{
    public static SeasonalAdjustmentReviewValidator DefaultSeasonalAdjustmentReviewValidator()
    {
        return new SeasonalAdjustmentReviewValidator(
            new List<ISeasonalAdjustmentReviewRule>
            {
                new SeasonalAdjustmentReviewUpdatedAtRangeRule(120, 120),
                new SeasonalAdjustmentReviewCodePrefixRule("Q"),
                new SeasonalAdjustmentReviewTagsLimitRule(32, new List<string> { "default" }),
                new SeasonalAdjustmentReviewTitleLengthRule(32),
                new SeasonalAdjustmentReviewRequiredNotesRule("seasonal adjustment review needs a value"),
                new SeasonalAdjustmentReviewCodeLengthRule(64),
            });
    }
}
