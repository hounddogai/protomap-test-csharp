namespace Acme.Workforce.Engagement.Feedback.Score;

/// <summary>A rule a global feedback variance must pass. Check returns its problems, or none.</summary>
public interface IGlobalFeedbackVarianceRule
{
    List<string> Check(GlobalFeedbackVariance item);
}

public sealed record GlobalFeedbackVarianceRequiredOwnerIdRule(string Message) : IGlobalFeedbackVarianceRule
{
    public List<string> Check(GlobalFeedbackVariance item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalFeedbackVarianceLocationLengthRule(int MaxLength) : IGlobalFeedbackVarianceRule
{
    public List<string> Check(GlobalFeedbackVariance item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalFeedbackVarianceRequiredLocationRule(string Message) : IGlobalFeedbackVarianceRule
{
    public List<string> Check(GlobalFeedbackVariance item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global feedback variance.</summary>
public sealed record GlobalFeedbackVarianceValidator(List<IGlobalFeedbackVarianceRule> Rules)
{
    public List<string> Validate(GlobalFeedbackVariance item)
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

    public void RequireValid(GlobalFeedbackVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalFeedbackVariance> items)
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

/// <summary>Functions of global feedback variance rules.</summary>
public static class GlobalFeedbackVarianceRules
{
    public static GlobalFeedbackVarianceValidator DefaultGlobalFeedbackVarianceValidator()
    {
        return new GlobalFeedbackVarianceValidator(
            new List<IGlobalFeedbackVarianceRule>
            {
                new GlobalFeedbackVarianceRequiredOwnerIdRule("global feedback variance needs a value"),
                new GlobalFeedbackVarianceLocationLengthRule(16),
                new GlobalFeedbackVarianceRequiredLocationRule("global feedback variance needs a value"),
            });
    }
}
