namespace Acme.Workforce.Engagement.Recognition.Template;

/// <summary>A rule a capped recognition template must pass. Check returns its problems, or none.</summary>
public interface ICappedRecognitionTemplateRule
{
    List<string> Check(CappedRecognitionTemplate item);
}

public sealed record CappedRecognitionTemplateCodePrefixRule(string Prefix) : ICappedRecognitionTemplateRule
{
    public List<string> Check(CappedRecognitionTemplate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CappedRecognitionTemplateRateRangeRule(double Min, double Max) : ICappedRecognitionTemplateRule
{
    public List<string> Check(CappedRecognitionTemplate item)
    {
        var valueValue = item.Rate;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rate {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rate {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CappedRecognitionTemplateRatingRangeRule(double Min, double Max) : ICappedRecognitionTemplateRule
{
    public List<string> Check(CappedRecognitionTemplate item)
    {
        var valueValue = item.Rating;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rating {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rating {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CappedRecognitionTemplateRequiredRegionRule(string Message) : ICappedRecognitionTemplateRule
{
    public List<string> Check(CappedRecognitionTemplate item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedRecognitionTemplateRevisionRangeRule(int Min, int Max) : ICappedRecognitionTemplateRule
{
    public List<string> Check(CappedRecognitionTemplate item)
    {
        var valueValue = item.Revision;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"revision {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"revision {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a capped recognition template.</summary>
public sealed record CappedRecognitionTemplateValidator(List<ICappedRecognitionTemplateRule> Rules)
{
    public List<string> Validate(CappedRecognitionTemplate item)
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

    public void RequireValid(CappedRecognitionTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedRecognitionTemplate> items)
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

/// <summary>Functions of capped recognition template rules.</summary>
public static class CappedRecognitionTemplateRules
{
    public static CappedRecognitionTemplateValidator DefaultCappedRecognitionTemplateValidator()
    {
        return new CappedRecognitionTemplateValidator(
            new List<ICappedRecognitionTemplateRule>
            {
                new CappedRecognitionTemplateCodePrefixRule("ACM"),
                new CappedRecognitionTemplateRateRangeRule(6.8, 2655.4),
                new CappedRecognitionTemplateRatingRangeRule(6.6, 4468.8),
                new CappedRecognitionTemplateRequiredRegionRule("capped recognition template needs an entry"),
                new CappedRecognitionTemplateRevisionRangeRule(120, 86400),
            });
    }
}
