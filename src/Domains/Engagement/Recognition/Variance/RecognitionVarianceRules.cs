namespace Acme.Workforce.Engagement.Recognition.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a recognition variance must pass. Check returns its problems, or none.</summary>
public interface IRecognitionVarianceRule
{
    List<string> Check(RecognitionVariance item);
}

public sealed record RecognitionVarianceAllowedTitleRule(List<string> Allowed) : IRecognitionVarianceRule
{
    public List<string> Check(RecognitionVariance item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionVarianceRequiredTitleRule(string Message) : IRecognitionVarianceRule
{
    public List<string> Check(RecognitionVariance item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RecognitionVarianceCodePrefixRule(string Prefix) : IRecognitionVarianceRule
{
    public List<string> Check(RecognitionVariance item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionVarianceCodeLengthRule(int MaxLength) : IRecognitionVarianceRule
{
    public List<string> Check(RecognitionVariance item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionVarianceTitleLengthRule(int MaxLength) : IRecognitionVarianceRule
{
    public List<string> Check(RecognitionVariance item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a recognition variance.</summary>
public sealed record RecognitionVarianceValidator(List<IRecognitionVarianceRule> Rules)
{
    public List<string> Validate(RecognitionVariance item)
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

    public void RequireValid(RecognitionVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RecognitionVariance> items)
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

/// <summary>Functions of recognition variance rules.</summary>
public static class RecognitionVarianceRules
{
    public static RecognitionVarianceValidator DefaultRecognitionVarianceValidator()
    {
        return new RecognitionVarianceValidator(
            new List<IRecognitionVarianceRule>
            {
                new RecognitionVarianceAllowedTitleRule(new List<string> { "batched", "scheduled" }),
                new RecognitionVarianceRequiredTitleRule("recognition variance needs an entry"),
                new RecognitionVarianceCodePrefixRule("X"),
                new RecognitionVarianceCodeLengthRule(32),
                new RecognitionVarianceTitleLengthRule(120),
            });
    }
}
