namespace Acme.Workforce.Compensation.PayBand.Forecast;

/// <summary>A rule a scheduled pay band forecast must pass. Check returns its problems, or none.</summary>
public interface IScheduledPayBandForecastRule
{
    List<string> Check(ScheduledPayBandForecast item);
}

public sealed record ScheduledPayBandForecastNameLengthRule(int MaxLength) : IScheduledPayBandForecastRule
{
    public List<string> Check(ScheduledPayBandForecast item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPayBandForecastRequiredCodeRule(string Message) : IScheduledPayBandForecastRule
{
    public List<string> Check(ScheduledPayBandForecast item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPayBandForecastTeamLengthRule(int MaxLength) : IScheduledPayBandForecastRule
{
    public List<string> Check(ScheduledPayBandForecast item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPayBandForecastRequiredNameRule(string Message) : IScheduledPayBandForecastRule
{
    public List<string> Check(ScheduledPayBandForecast item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled pay band forecast.</summary>
public sealed record ScheduledPayBandForecastValidator(List<IScheduledPayBandForecastRule> Rules)
{
    public List<string> Validate(ScheduledPayBandForecast item)
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

    public void RequireValid(ScheduledPayBandForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledPayBandForecast> items)
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

/// <summary>Functions of scheduled pay band forecast rules.</summary>
public static class ScheduledPayBandForecastRules
{
    public static ScheduledPayBandForecastValidator DefaultScheduledPayBandForecastValidator()
    {
        return new ScheduledPayBandForecastValidator(
            new List<IScheduledPayBandForecastRule>
            {
                new ScheduledPayBandForecastNameLengthRule(255),
                new ScheduledPayBandForecastRequiredCodeRule("scheduled pay band forecast needs an entry"),
                new ScheduledPayBandForecastTeamLengthRule(86400),
                new ScheduledPayBandForecastRequiredNameRule("scheduled pay band forecast needs an entry"),
            });
    }
}
