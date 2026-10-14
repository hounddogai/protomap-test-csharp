namespace Acme.Workforce.Engagement.Recognition.Claim;

/// <summary>A rule a recognition group must pass. Check returns its problems, or none.</summary>
public interface IRecognitionGroupRule
{
    List<string> Check(RecognitionGroup item);
}

public sealed record RecognitionGroupRequiredTitleRule(string Message) : IRecognitionGroupRule
{
    public List<string> Check(RecognitionGroup item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RecognitionGroupCodePrefixRule(string Prefix) : IRecognitionGroupRule
{
    public List<string> Check(RecognitionGroup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionGroupTitleLengthRule(int MaxLength) : IRecognitionGroupRule
{
    public List<string> Check(RecognitionGroup item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a recognition group.</summary>
public sealed record RecognitionGroupValidator(List<IRecognitionGroupRule> Rules)
{
    public List<string> Validate(RecognitionGroup item)
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

    public void RequireValid(RecognitionGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RecognitionGroup> items)
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

/// <summary>Functions of recognition group rules.</summary>
public static class RecognitionGroupRules
{
    public static RecognitionGroupValidator DefaultRecognitionGroupValidator()
    {
        return new RecognitionGroupValidator(
            new List<IRecognitionGroupRule>
            {
                new RecognitionGroupRequiredTitleRule("recognition group needs this field"),
                new RecognitionGroupCodePrefixRule("Q"),
                new RecognitionGroupTitleLengthRule(32),
            });
    }
}
