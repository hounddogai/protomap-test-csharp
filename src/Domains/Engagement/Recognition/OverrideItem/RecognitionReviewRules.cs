namespace Acme.Workforce.Engagement.Recognition.OverrideItem;

/// <summary>A rule a recognition review must pass. Check returns its problems, or none.</summary>
public interface IRecognitionReviewRule
{
    List<string> Check(RecognitionReview item);
}

public sealed record RecognitionReviewOwnerIdLengthRule(int MaxLength) : IRecognitionReviewRule
{
    public List<string> Check(RecognitionReview item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionReviewCodePrefixRule(string Prefix) : IRecognitionReviewRule
{
    public List<string> Check(RecognitionReview item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionReviewTitleLengthRule(int MaxLength) : IRecognitionReviewRule
{
    public List<string> Check(RecognitionReview item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionReviewRequiredCodeRule(string Message) : IRecognitionReviewRule
{
    public List<string> Check(RecognitionReview item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RecognitionReviewLimitRangeRule(int Min, int Max) : IRecognitionReviewRule
{
    public List<string> Check(RecognitionReview item)
    {
        var valueValue = item.Limit;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"limit {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionReviewCodeLengthRule(int MaxLength) : IRecognitionReviewRule
{
    public List<string> Check(RecognitionReview item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a recognition review.</summary>
public sealed record RecognitionReviewValidator(List<IRecognitionReviewRule> Rules)
{
    public List<string> Validate(RecognitionReview item)
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

    public void RequireValid(RecognitionReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RecognitionReview> items)
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

/// <summary>Functions of recognition review rules.</summary>
public static class RecognitionReviewRules
{
    public static RecognitionReviewValidator DefaultRecognitionReviewValidator()
    {
        return new RecognitionReviewValidator(
            new List<IRecognitionReviewRule>
            {
                new RecognitionReviewOwnerIdLengthRule(120),
                new RecognitionReviewCodePrefixRule("OPS"),
                new RecognitionReviewTitleLengthRule(64),
                new RecognitionReviewRequiredCodeRule("recognition review needs an entry"),
                new RecognitionReviewLimitRangeRule(16, 120),
                new RecognitionReviewCodeLengthRule(86400),
            });
    }
}
