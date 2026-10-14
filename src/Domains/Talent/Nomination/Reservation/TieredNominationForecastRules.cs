namespace Acme.Workforce.Talent.Nomination.Reservation;

/// <summary>A rule a tiered nomination forecast must pass. Check returns its problems, or none.</summary>
public interface ITieredNominationForecastRule
{
    List<string> Check(TieredNominationForecast item);
}

public sealed record TieredNominationForecastRequiredCodeRule(string Message) : ITieredNominationForecastRule
{
    public List<string> Check(TieredNominationForecast item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredNominationForecastJobCodeLengthRule(int MaxLength) : ITieredNominationForecastRule
{
    public List<string> Check(TieredNominationForecast item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredNominationForecastRetriesRangeRule(int Min, int Max) : ITieredNominationForecastRule
{
    public List<string> Check(TieredNominationForecast item)
    {
        var valueValue = item.Retries;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"retries {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"retries {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TieredNominationForecastCodePrefixRule(string Prefix) : ITieredNominationForecastRule
{
    public List<string> Check(TieredNominationForecast item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TieredNominationForecastCodeLengthRule(int MaxLength) : ITieredNominationForecastRule
{
    public List<string> Check(TieredNominationForecast item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered nomination forecast.</summary>
public sealed record TieredNominationForecastValidator(List<ITieredNominationForecastRule> Rules)
{
    public List<string> Validate(TieredNominationForecast item)
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

    public void RequireValid(TieredNominationForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredNominationForecast> items)
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

/// <summary>Functions of tiered nomination forecast rules.</summary>
public static class TieredNominationForecastRules
{
    public static TieredNominationForecastValidator DefaultTieredNominationForecastValidator()
    {
        return new TieredNominationForecastValidator(
            new List<ITieredNominationForecastRule>
            {
                new TieredNominationForecastRequiredCodeRule("tiered nomination forecast needs a value"),
                new TieredNominationForecastJobCodeLengthRule(80),
                new TieredNominationForecastRetriesRangeRule(64, 80),
                new TieredNominationForecastCodePrefixRule("X"),
                new TieredNominationForecastCodeLengthRule(86400),
            });
    }
}
