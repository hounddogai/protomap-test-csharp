namespace Acme.Workforce.Engagement.Sentiment.Proposal;

/// <summary>A rule a default sentiment assignment must pass. Check returns its problems, or none.</summary>
public interface IDefaultSentimentAssignmentRule
{
    List<string> Check(DefaultSentimentAssignment item);
}

public sealed record DefaultSentimentAssignmentEmployeeIdLengthRule(int MaxLength) : IDefaultSentimentAssignmentRule
{
    public List<string> Check(DefaultSentimentAssignment item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultSentimentAssignmentCodePrefixRule(string Prefix) : IDefaultSentimentAssignmentRule
{
    public List<string> Check(DefaultSentimentAssignment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultSentimentAssignmentRequiredCodeRule(string Message) : IDefaultSentimentAssignmentRule
{
    public List<string> Check(DefaultSentimentAssignment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultSentimentAssignmentCategoryLengthRule(int MaxLength) : IDefaultSentimentAssignmentRule
{
    public List<string> Check(DefaultSentimentAssignment item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default sentiment assignment.</summary>
public sealed record DefaultSentimentAssignmentValidator(List<IDefaultSentimentAssignmentRule> Rules)
{
    public List<string> Validate(DefaultSentimentAssignment item)
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

    public void RequireValid(DefaultSentimentAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultSentimentAssignment> items)
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

/// <summary>Functions of default sentiment assignment rules.</summary>
public static class DefaultSentimentAssignmentRules
{
    public static DefaultSentimentAssignmentValidator DefaultDefaultSentimentAssignmentValidator()
    {
        return new DefaultSentimentAssignmentValidator(
            new List<IDefaultSentimentAssignmentRule>
            {
                new DefaultSentimentAssignmentEmployeeIdLengthRule(32),
                new DefaultSentimentAssignmentCodePrefixRule("OPS"),
                new DefaultSentimentAssignmentRequiredCodeRule("default sentiment assignment needs a value"),
                new DefaultSentimentAssignmentCategoryLengthRule(16),
            });
    }
}
