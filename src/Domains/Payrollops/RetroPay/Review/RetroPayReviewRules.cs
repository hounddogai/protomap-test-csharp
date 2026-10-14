namespace Acme.Workforce.Payrollops.RetroPay.Review;

/// <summary>A rule a retro pay review must pass. Check returns its problems, or none.</summary>
public interface IRetroPayReviewRule
{
    List<string> Check(RetroPayReview item);
}

public sealed record RetroPayReviewTimezoneLengthRule(int MaxLength) : IRetroPayReviewRule
{
    public List<string> Check(RetroPayReview item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayReviewCodeLengthRule(int MaxLength) : IRetroPayReviewRule
{
    public List<string> Check(RetroPayReview item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayReviewTitleLengthRule(int MaxLength) : IRetroPayReviewRule
{
    public List<string> Check(RetroPayReview item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayReviewAttemptsRangeRule(int Min, int Max) : IRetroPayReviewRule
{
    public List<string> Check(RetroPayReview item)
    {
        var valueValue = item.Attempts;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"attempts {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"attempts {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a retro pay review.</summary>
public sealed record RetroPayReviewValidator(List<IRetroPayReviewRule> Rules)
{
    public List<string> Validate(RetroPayReview item)
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

    public void RequireValid(RetroPayReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RetroPayReview> items)
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

/// <summary>Functions of retro pay review rules.</summary>
public static class RetroPayReviewRules
{
    public static RetroPayReviewValidator DefaultRetroPayReviewValidator()
    {
        return new RetroPayReviewValidator(
            new List<IRetroPayReviewRule>
            {
                new RetroPayReviewTimezoneLengthRule(255),
                new RetroPayReviewCodeLengthRule(32),
                new RetroPayReviewTitleLengthRule(80),
                new RetroPayReviewAttemptsRangeRule(32, 80),
            });
    }
}
