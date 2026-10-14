namespace Acme.Workforce.Engagement.Feedback.Rule;

/// <summary>A rule a default feedback rule must pass. Check returns its problems, or none.</summary>
public interface IDefaultFeedbackRuleRule
{
    List<string> Check(DefaultFeedbackRule item);
}

public sealed record DefaultFeedbackRuleCodeLengthRule(int MaxLength) : IDefaultFeedbackRuleRule
{
    public List<string> Check(DefaultFeedbackRule item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultFeedbackRuleCreatedAtRangeRule(int Min, int Max) : IDefaultFeedbackRuleRule
{
    public List<string> Check(DefaultFeedbackRule item)
    {
        var valueValue = item.CreatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"created at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultFeedbackRuleCodePrefixRule(string Prefix) : IDefaultFeedbackRuleRule
{
    public List<string> Check(DefaultFeedbackRule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default feedback rule.</summary>
public sealed record DefaultFeedbackRuleValidatorExtra(List<IDefaultFeedbackRuleRule> Rules)
{
    public List<string> Validate(DefaultFeedbackRule item)
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

    public void RequireValid(DefaultFeedbackRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultFeedbackRule> items)
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

/// <summary>Functions of default feedback rule rules.</summary>
public static class DefaultFeedbackRuleRules
{
    public static DefaultFeedbackRuleValidatorExtra DefaultDefaultFeedbackRuleValidator()
    {
        return new DefaultFeedbackRuleValidatorExtra(
            new List<IDefaultFeedbackRuleRule>
            {
                new DefaultFeedbackRuleCodeLengthRule(86400),
                new DefaultFeedbackRuleCreatedAtRangeRule(80, 86400),
                new DefaultFeedbackRuleCodePrefixRule("Q"),
            });
    }
}
