namespace Acme.Workforce.Engagement.Recognition.Template;

/// <summary>A rule a weighted recognition variance must pass. Check returns its problems, or none.</summary>
public interface IWeightedRecognitionVarianceRule
{
    List<string> Check(WeightedRecognitionVariance item);
}

public sealed record WeightedRecognitionVarianceCodePrefixRule(string Prefix) : IWeightedRecognitionVarianceRule
{
    public List<string> Check(WeightedRecognitionVariance item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedRecognitionVarianceChannelLengthRule(int MaxLength) : IWeightedRecognitionVarianceRule
{
    public List<string> Check(WeightedRecognitionVariance item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedRecognitionVarianceRequiredChannelRule(string Message) : IWeightedRecognitionVarianceRule
{
    public List<string> Check(WeightedRecognitionVariance item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedRecognitionVarianceRequiredContactEmailRule(string Message) : IWeightedRecognitionVarianceRule
{
    public List<string> Check(WeightedRecognitionVariance item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedRecognitionVarianceRequiredLabelRule(string Message) : IWeightedRecognitionVarianceRule
{
    public List<string> Check(WeightedRecognitionVariance item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted recognition variance.</summary>
public sealed record WeightedRecognitionVarianceValidator(List<IWeightedRecognitionVarianceRule> Rules)
{
    public List<string> Validate(WeightedRecognitionVariance item)
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

    public void RequireValid(WeightedRecognitionVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedRecognitionVariance> items)
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

/// <summary>Functions of weighted recognition variance rules.</summary>
public static class WeightedRecognitionVarianceRules
{
    public static WeightedRecognitionVarianceValidator DefaultWeightedRecognitionVarianceValidator()
    {
        return new WeightedRecognitionVarianceValidator(
            new List<IWeightedRecognitionVarianceRule>
            {
                new WeightedRecognitionVarianceCodePrefixRule("X"),
                new WeightedRecognitionVarianceChannelLengthRule(120),
                new WeightedRecognitionVarianceRequiredChannelRule("weighted recognition variance needs this field"),
                new WeightedRecognitionVarianceRequiredContactEmailRule(
                    "weighted recognition variance needs an entry"),
                new WeightedRecognitionVarianceRequiredLabelRule("weighted recognition variance needs this field"),
            });
    }
}
