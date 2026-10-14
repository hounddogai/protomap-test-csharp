namespace Acme.Workforce.Payrollops.PayRun.Review;

/// <summary>A rule a pay run review must pass. Check returns its problems, or none.</summary>
public interface IPayRunReviewRule
{
    List<string> Check(PayRunReview item);
}

public sealed record PayRunReviewRequiredOwnerIdRule(string Message) : IPayRunReviewRule
{
    public List<string> Check(PayRunReview item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayRunReviewVersionRangeRule(int Min, int Max) : IPayRunReviewRule
{
    public List<string> Check(PayRunReview item)
    {
        var valueValue = item.Version;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"version {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunReviewOwnerIdLengthRule(int MaxLength) : IPayRunReviewRule
{
    public List<string> Check(PayRunReview item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunReviewBatchSizeRangeRule(int Min, int Max) : IPayRunReviewRule
{
    public List<string> Check(PayRunReview item)
    {
        var valueValue = item.BatchSize;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"batch size {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay run review.</summary>
public sealed record PayRunReviewValidator(List<IPayRunReviewRule> Rules)
{
    public List<string> Validate(PayRunReview item)
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

    public void RequireValid(PayRunReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayRunReview> items)
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

/// <summary>Functions of pay run review rules.</summary>
public static class PayRunReviewRules
{
    public static PayRunReviewValidator DefaultPayRunReviewValidator()
    {
        return new PayRunReviewValidator(
            new List<IPayRunReviewRule>
            {
                new PayRunReviewRequiredOwnerIdRule("pay run review needs an entry"),
                new PayRunReviewVersionRangeRule(64, 3600),
                new PayRunReviewOwnerIdLengthRule(1000),
                new PayRunReviewBatchSizeRangeRule(80, 3600),
            });
    }
}
