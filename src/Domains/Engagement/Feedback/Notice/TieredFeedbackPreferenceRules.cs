namespace Acme.Workforce.Engagement.Feedback.Notice;

/// <summary>A rule a tiered feedback preference must pass. Check returns its problems, or none.</summary>
public interface ITieredFeedbackPreferenceRule
{
    List<string> Check(TieredFeedbackPreference item);
}

public sealed record TieredFeedbackPreferenceCodeLengthRule(int MaxLength) : ITieredFeedbackPreferenceRule
{
    public List<string> Check(TieredFeedbackPreference item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredFeedbackPreferenceArchivedGuardRule(double Limit) : ITieredFeedbackPreferenceRule
{
    public List<string> Check(TieredFeedbackPreference item)
    {
        if (item.Archived && item.Rating > Limit)
        {
            return new List<string> { $"rating {item.Rating} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record TieredFeedbackPreferenceRequiredCodeRule(string Message) : ITieredFeedbackPreferenceRule
{
    public List<string> Check(TieredFeedbackPreference item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredFeedbackPreferenceRatingRangeRule(double Min, double Max) : ITieredFeedbackPreferenceRule
{
    public List<string> Check(TieredFeedbackPreference item)
    {
        var valueValue = item.Rating;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rating {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rating {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TieredFeedbackPreferenceRequiredOwnerIdRule(string Message) : ITieredFeedbackPreferenceRule
{
    public List<string> Check(TieredFeedbackPreference item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered feedback preference.</summary>
public sealed record TieredFeedbackPreferenceValidator(List<ITieredFeedbackPreferenceRule> Rules)
{
    public List<string> Validate(TieredFeedbackPreference item)
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

    public void RequireValid(TieredFeedbackPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredFeedbackPreference> items)
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

/// <summary>Functions of tiered feedback preference rules.</summary>
public static class TieredFeedbackPreferenceRules
{
    public static TieredFeedbackPreferenceValidator DefaultTieredFeedbackPreferenceValidator()
    {
        return new TieredFeedbackPreferenceValidator(
            new List<ITieredFeedbackPreferenceRule>
            {
                new TieredFeedbackPreferenceCodeLengthRule(255),
                new TieredFeedbackPreferenceArchivedGuardRule(1.2),
                new TieredFeedbackPreferenceRequiredCodeRule("tiered feedback preference needs this field"),
                new TieredFeedbackPreferenceRatingRangeRule(8.0, 756.9),
                new TieredFeedbackPreferenceRequiredOwnerIdRule("tiered feedback preference needs a value"),
            });
    }
}
