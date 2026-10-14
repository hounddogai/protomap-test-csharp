namespace Acme.Workforce.Engagement.PulseSurvey.Metric;

/// <summary>A rule a manual pulse survey forecast must pass. Check returns its problems, or none.</summary>
public interface IManualPulseSurveyForecastRule
{
    List<string> Check(ManualPulseSurveyForecast item);
}

public sealed record ManualPulseSurveyForecastManagerIdLengthRule(int MaxLength) : IManualPulseSurveyForecastRule
{
    public List<string> Check(ManualPulseSurveyForecast item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualPulseSurveyForecastRequiredCodeRule(string Message) : IManualPulseSurveyForecastRule
{
    public List<string> Check(ManualPulseSurveyForecast item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualPulseSurveyForecastLabelLengthRule(int MaxLength) : IManualPulseSurveyForecastRule
{
    public List<string> Check(ManualPulseSurveyForecast item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual pulse survey forecast.</summary>
public sealed record ManualPulseSurveyForecastValidator(List<IManualPulseSurveyForecastRule> Rules)
{
    public List<string> Validate(ManualPulseSurveyForecast item)
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

    public void RequireValid(ManualPulseSurveyForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualPulseSurveyForecast> items)
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

/// <summary>Functions of manual pulse survey forecast rules.</summary>
public static class ManualPulseSurveyForecastRules
{
    public static ManualPulseSurveyForecastValidator DefaultManualPulseSurveyForecastValidator()
    {
        return new ManualPulseSurveyForecastValidator(
            new List<IManualPulseSurveyForecastRule>
            {
                new ManualPulseSurveyForecastManagerIdLengthRule(16),
                new ManualPulseSurveyForecastRequiredCodeRule("manual pulse survey forecast needs a value"),
                new ManualPulseSurveyForecastLabelLengthRule(16),
            });
    }
}
