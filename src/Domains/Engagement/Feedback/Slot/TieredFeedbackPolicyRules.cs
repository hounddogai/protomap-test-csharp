namespace Acme.Workforce.Engagement.Feedback.Slot;

/// <summary>A rule a tiered feedback policy must pass. Check returns its problems, or none.</summary>
public interface ITieredFeedbackPolicyRule
{
    List<string> Check(TieredFeedbackPolicy item);
}

public sealed record TieredFeedbackPolicyCostRangeRule(double Min, double Max) : ITieredFeedbackPolicyRule
{
    public List<string> Check(TieredFeedbackPolicy item)
    {
        var valueValue = item.Cost;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"cost {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"cost {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TieredFeedbackPolicyCodePrefixRule(string Prefix) : ITieredFeedbackPolicyRule
{
    public List<string> Check(TieredFeedbackPolicy item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TieredFeedbackPolicyRequiredCodeRule(string Message) : ITieredFeedbackPolicyRule
{
    public List<string> Check(TieredFeedbackPolicy item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredFeedbackPolicyRequiredTitleRule(string Message) : ITieredFeedbackPolicyRule
{
    public List<string> Check(TieredFeedbackPolicy item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredFeedbackPolicyArchivedGuardRule(double Limit) : ITieredFeedbackPolicyRule
{
    public List<string> Check(TieredFeedbackPolicy item)
    {
        if (item.Archived && item.Rate > Limit)
        {
            return new List<string> { $"rate {item.Rate} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record TieredFeedbackPolicyDescriptionLengthRule(int MaxLength) : ITieredFeedbackPolicyRule
{
    public List<string> Check(TieredFeedbackPolicy item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered feedback policy.</summary>
public sealed record TieredFeedbackPolicyValidator(List<ITieredFeedbackPolicyRule> Rules)
{
    public List<string> Validate(TieredFeedbackPolicy item)
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

    public void RequireValid(TieredFeedbackPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredFeedbackPolicy> items)
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

/// <summary>Functions of tiered feedback policy rules.</summary>
public static class TieredFeedbackPolicyRules
{
    public static TieredFeedbackPolicyValidator DefaultTieredFeedbackPolicyValidator()
    {
        return new TieredFeedbackPolicyValidator(
            new List<ITieredFeedbackPolicyRule>
            {
                new TieredFeedbackPolicyCostRangeRule(8.0, 3860.7),
                new TieredFeedbackPolicyCodePrefixRule("Q"),
                new TieredFeedbackPolicyRequiredCodeRule("tiered feedback policy needs a value"),
                new TieredFeedbackPolicyRequiredTitleRule("tiered feedback policy needs an entry"),
                new TieredFeedbackPolicyArchivedGuardRule(1.6),
                new TieredFeedbackPolicyDescriptionLengthRule(120),
            });
    }
}
