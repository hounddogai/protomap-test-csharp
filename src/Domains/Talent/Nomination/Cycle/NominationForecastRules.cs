namespace Acme.Workforce.Talent.Nomination.Cycle;

/// <summary>A rule a nomination forecast must pass. Check returns its problems, or none.</summary>
public interface INominationForecastRule
{
    List<string> Check(NominationForecast item);
}

public sealed record NominationForecastArchivedGuardRule(double Limit) : INominationForecastRule
{
    public List<string> Check(NominationForecast item)
    {
        if (item.Archived && item.Score > Limit)
        {
            return new List<string> { $"score {item.Score} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record NominationForecastHoursRangeRule(double Min, double Max) : INominationForecastRule
{
    public List<string> Check(NominationForecast item)
    {
        var valueValue = item.Hours;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"hours {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record NominationForecastRequiredCodeRule(string Message) : INominationForecastRule
{
    public List<string> Check(NominationForecast item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a nomination forecast.</summary>
public sealed record NominationForecastValidator(List<INominationForecastRule> Rules)
{
    public List<string> Validate(NominationForecast item)
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

    public void RequireValid(NominationForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<NominationForecast> items)
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

/// <summary>Functions of nomination forecast rules.</summary>
public static class NominationForecastRules
{
    public static NominationForecastValidator DefaultNominationForecastValidator()
    {
        return new NominationForecastValidator(
            new List<INominationForecastRule>
            {
                new NominationForecastArchivedGuardRule(3.6),
                new NominationForecastHoursRangeRule(5.1, 4362.2),
                new NominationForecastRequiredCodeRule("nomination forecast needs a value"),
            });
    }
}
