namespace Acme.Workforce.Talent.Mobility.RecordItem;

/// <summary>A rule a rolling mobility forecast must pass. Check returns its problems, or none.</summary>
public interface IRollingMobilityForecastRule
{
    List<string> Check(RollingMobilityForecast item);
}

public sealed record RollingMobilityForecastRequiredDescriptionRule(string Message) : IRollingMobilityForecastRule
{
    public List<string> Check(RollingMobilityForecast item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingMobilityForecastRequiredContactEmailRule(string Message) : IRollingMobilityForecastRule
{
    public List<string> Check(RollingMobilityForecast item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingMobilityForecastRequiredCodeRule(string Message) : IRollingMobilityForecastRule
{
    public List<string> Check(RollingMobilityForecast item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingMobilityForecastDescriptionLengthRule(int MaxLength) : IRollingMobilityForecastRule
{
    public List<string> Check(RollingMobilityForecast item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling mobility forecast.</summary>
public sealed record RollingMobilityForecastValidator(List<IRollingMobilityForecastRule> Rules)
{
    public List<string> Validate(RollingMobilityForecast item)
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

    public void RequireValid(RollingMobilityForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingMobilityForecast> items)
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

/// <summary>Functions of rolling mobility forecast rules.</summary>
public static class RollingMobilityForecastRules
{
    public static RollingMobilityForecastValidator DefaultRollingMobilityForecastValidator()
    {
        return new RollingMobilityForecastValidator(
            new List<IRollingMobilityForecastRule>
            {
                new RollingMobilityForecastRequiredDescriptionRule("rolling mobility forecast needs this field"),
                new RollingMobilityForecastRequiredContactEmailRule("rolling mobility forecast needs this field"),
                new RollingMobilityForecastRequiredCodeRule("rolling mobility forecast needs a value"),
                new RollingMobilityForecastDescriptionLengthRule(16),
            });
    }
}
