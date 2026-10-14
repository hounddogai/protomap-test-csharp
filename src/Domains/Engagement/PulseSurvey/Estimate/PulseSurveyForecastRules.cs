namespace Acme.Workforce.Engagement.PulseSurvey.Estimate;

/// <summary>A rule a pulse survey forecast must pass. Check returns its problems, or none.</summary>
public interface IPulseSurveyForecastRule
{
    List<string> Check(PulseSurveyForecast item);
}

public sealed record PulseSurveyForecastChannelLengthRule(int MaxLength) : IPulseSurveyForecastRule
{
    public List<string> Check(PulseSurveyForecast item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyForecastCodePrefixRule(string Prefix) : IPulseSurveyForecastRule
{
    public List<string> Check(PulseSurveyForecast item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyForecastCodeLengthRule(int MaxLength) : IPulseSurveyForecastRule
{
    public List<string> Check(PulseSurveyForecast item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyForecastRequiredTitleRule(string Message) : IPulseSurveyForecastRule
{
    public List<string> Check(PulseSurveyForecast item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyForecastRequiredCodeRule(string Message) : IPulseSurveyForecastRule
{
    public List<string> Check(PulseSurveyForecast item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pulse survey forecast.</summary>
public sealed record PulseSurveyForecastValidator(List<IPulseSurveyForecastRule> Rules)
{
    public List<string> Validate(PulseSurveyForecast item)
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

    public void RequireValid(PulseSurveyForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PulseSurveyForecast> items)
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

/// <summary>Functions of pulse survey forecast rules.</summary>
public static class PulseSurveyForecastRules
{
    public static PulseSurveyForecastValidator DefaultPulseSurveyForecastValidator()
    {
        return new PulseSurveyForecastValidator(
            new List<IPulseSurveyForecastRule>
            {
                new PulseSurveyForecastChannelLengthRule(32),
                new PulseSurveyForecastCodePrefixRule("OPS"),
                new PulseSurveyForecastCodeLengthRule(16),
                new PulseSurveyForecastRequiredTitleRule("pulse survey forecast needs this field"),
                new PulseSurveyForecastRequiredCodeRule("pulse survey forecast needs a value"),
            });
    }
}
