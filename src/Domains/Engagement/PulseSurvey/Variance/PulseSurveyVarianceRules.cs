namespace Acme.Workforce.Engagement.PulseSurvey.Variance;

/// <summary>A rule a pulse survey variance must pass. Check returns its problems, or none.</summary>
public interface IPulseSurveyVarianceRule
{
    List<string> Check(PulseSurveyVariance item);
}

public sealed record PulseSurveyVarianceRateRangeRule(double Min, double Max) : IPulseSurveyVarianceRule
{
    public List<string> Check(PulseSurveyVariance item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyVarianceSourceLengthRule(int MaxLength) : IPulseSurveyVarianceRule
{
    public List<string> Check(PulseSurveyVariance item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyVarianceRequiredSourceRule(string Message) : IPulseSurveyVarianceRule
{
    public List<string> Check(PulseSurveyVariance item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyVarianceTagsLimitRule(int MaxCount, List<string> Banned) : IPulseSurveyVarianceRule
{
    public List<string> Check(PulseSurveyVariance item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

public sealed record PulseSurveyVarianceCurrencyLengthRule(int MaxLength) : IPulseSurveyVarianceRule
{
    public List<string> Check(PulseSurveyVariance item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pulse survey variance.</summary>
public sealed record PulseSurveyVarianceValidator(List<IPulseSurveyVarianceRule> Rules)
{
    public List<string> Validate(PulseSurveyVariance item)
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

    public void RequireValid(PulseSurveyVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PulseSurveyVariance> items)
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

/// <summary>Functions of pulse survey variance rules.</summary>
public static class PulseSurveyVarianceRules
{
    public static PulseSurveyVarianceValidator DefaultPulseSurveyVarianceValidator()
    {
        return new PulseSurveyVarianceValidator(
            new List<IPulseSurveyVarianceRule>
            {
                new PulseSurveyVarianceRateRangeRule(9.3, 465.9),
                new PulseSurveyVarianceSourceLengthRule(64),
                new PulseSurveyVarianceRequiredSourceRule("pulse survey variance needs a value"),
                new PulseSurveyVarianceTagsLimitRule(64, new List<string> { "global", "manual" }),
                new PulseSurveyVarianceCurrencyLengthRule(3600),
            });
    }
}
