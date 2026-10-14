namespace Acme.Workforce.Engagement.PulseSurvey.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a strict pulse survey variance must pass. Check returns its problems, or none.</summary>
public interface IStrictPulseSurveyVarianceRule
{
    List<string> Check(StrictPulseSurveyVariance item);
}

public sealed record StrictPulseSurveyVarianceAmountRangeRule(double Min, double Max) : IStrictPulseSurveyVarianceRule
{
    public List<string> Check(StrictPulseSurveyVariance item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record StrictPulseSurveyVarianceStartsAtRangeRule(int Min, int Max) : IStrictPulseSurveyVarianceRule
{
    public List<string> Check(StrictPulseSurveyVariance item)
    {
        var valueValue = item.StartsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"starts at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"starts at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record StrictPulseSurveyVarianceCodeLengthRule(int MaxLength) : IStrictPulseSurveyVarianceRule
{
    public List<string> Check(StrictPulseSurveyVariance item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictPulseSurveyVarianceAllowedTitleRule(List<string> Allowed) : IStrictPulseSurveyVarianceRule
{
    public List<string> Check(StrictPulseSurveyVariance item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record StrictPulseSurveyVarianceRequiredCodeRule(string Message) : IStrictPulseSurveyVarianceRule
{
    public List<string> Check(StrictPulseSurveyVariance item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictPulseSurveyVarianceApprovedGuardRule(int Limit) : IStrictPulseSurveyVarianceRule
{
    public List<string> Check(StrictPulseSurveyVariance item)
    {
        if (item.Approved && item.StartsAt > Limit)
        {
            return new List<string> { $"starts at {item.StartsAt} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict pulse survey variance.</summary>
public sealed record StrictPulseSurveyVarianceValidator(List<IStrictPulseSurveyVarianceRule> Rules)
{
    public List<string> Validate(StrictPulseSurveyVariance item)
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

    public void RequireValid(StrictPulseSurveyVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictPulseSurveyVariance> items)
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

/// <summary>Functions of strict pulse survey variance rules.</summary>
public static class StrictPulseSurveyVarianceRules
{
    public static StrictPulseSurveyVarianceValidator DefaultStrictPulseSurveyVarianceValidator()
    {
        return new StrictPulseSurveyVarianceValidator(
            new List<IStrictPulseSurveyVarianceRule>
            {
                new StrictPulseSurveyVarianceAmountRangeRule(7.0, 2864.7),
                new StrictPulseSurveyVarianceStartsAtRangeRule(16, 1000),
                new StrictPulseSurveyVarianceCodeLengthRule(16),
                new StrictPulseSurveyVarianceAllowedTitleRule(new List<string>()),
                new StrictPulseSurveyVarianceRequiredCodeRule("strict pulse survey variance needs an entry"),
                new StrictPulseSurveyVarianceApprovedGuardRule(32),
            });
    }
}
