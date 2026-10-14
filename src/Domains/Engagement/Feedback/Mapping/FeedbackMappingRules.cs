namespace Acme.Workforce.Engagement.Feedback.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a feedback mapping must pass. Check returns its problems, or none.</summary>
public interface IFeedbackMappingRule
{
    List<string> Check(FeedbackMapping item);
}

public sealed record FeedbackMappingDiscountRangeRule(double Min, double Max) : IFeedbackMappingRule
{
    public List<string> Check(FeedbackMapping item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackMappingAllowedCodeRule(List<string> Allowed) : IFeedbackMappingRule
{
    public List<string> Check(FeedbackMapping item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is missing" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackMappingRequiredReviewerRule(string Message) : IFeedbackMappingRule
{
    public List<string> Check(FeedbackMapping item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FeedbackMappingCodeLengthRule(int MaxLength) : IFeedbackMappingRule
{
    public List<string> Check(FeedbackMapping item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a feedback mapping.</summary>
public sealed record FeedbackMappingValidator(List<IFeedbackMappingRule> Rules)
{
    public List<string> Validate(FeedbackMapping item)
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

    public void RequireValid(FeedbackMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FeedbackMapping> items)
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

/// <summary>Functions of feedback mapping rules.</summary>
public static class FeedbackMappingRules
{
    public static FeedbackMappingValidator DefaultFeedbackMappingValidator()
    {
        return new FeedbackMappingValidator(
            new List<IFeedbackMappingRule>
            {
                new FeedbackMappingDiscountRangeRule(9.4, 4442.1),
                new FeedbackMappingAllowedCodeRule(new List<string> { "rolling", "batched" }),
                new FeedbackMappingRequiredReviewerRule("feedback mapping needs a value"),
                new FeedbackMappingCodeLengthRule(86400),
            });
    }
}
