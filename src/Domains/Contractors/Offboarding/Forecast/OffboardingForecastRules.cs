namespace Acme.Workforce.Contractors.Offboarding.Forecast;

/// <summary>A rule a offboarding forecast must pass. Check returns its problems, or none.</summary>
public interface IOffboardingForecastRule
{
    List<string> Check(OffboardingForecast item);
}

public sealed record OffboardingForecastRequiredDescriptionRule(string Message) : IOffboardingForecastRule
{
    public List<string> Check(OffboardingForecast item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OffboardingForecastLabelLengthRule(int MaxLength) : IOffboardingForecastRule
{
    public List<string> Check(OffboardingForecast item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingForecastDescriptionLengthRule(int MaxLength) : IOffboardingForecastRule
{
    public List<string> Check(OffboardingForecast item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingForecastCodeLengthRule(int MaxLength) : IOffboardingForecastRule
{
    public List<string> Check(OffboardingForecast item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a offboarding forecast.</summary>
public sealed record OffboardingForecastValidator(List<IOffboardingForecastRule> Rules)
{
    public List<string> Validate(OffboardingForecast item)
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

    public void RequireValid(OffboardingForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OffboardingForecast> items)
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

/// <summary>Functions of offboarding forecast rules.</summary>
public static class OffboardingForecastRules
{
    public static OffboardingForecastValidator DefaultOffboardingForecastValidator()
    {
        return new OffboardingForecastValidator(
            new List<IOffboardingForecastRule>
            {
                new OffboardingForecastRequiredDescriptionRule("offboarding forecast needs this field"),
                new OffboardingForecastLabelLengthRule(64),
                new OffboardingForecastDescriptionLengthRule(1000),
                new OffboardingForecastCodeLengthRule(16),
            });
    }
}
