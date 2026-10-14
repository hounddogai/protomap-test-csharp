namespace Acme.Workforce.Workplace.Occupancy.Draft;

/// <summary>A rule a global occupancy forecast must pass. Check returns its problems, or none.</summary>
public interface IGlobalOccupancyForecastRule
{
    List<string> Check(GlobalOccupancyForecast item);
}

public sealed record GlobalOccupancyForecastActiveGuardRule(double Limit) : IGlobalOccupancyForecastRule
{
    public List<string> Check(GlobalOccupancyForecast item)
    {
        if (item.Active && item.Ceiling > Limit)
        {
            return new List<string> { $"ceiling {item.Ceiling} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record GlobalOccupancyForecastLabelLengthRule(int MaxLength) : IGlobalOccupancyForecastRule
{
    public List<string> Check(GlobalOccupancyForecast item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalOccupancyForecastCodePrefixRule(string Prefix) : IGlobalOccupancyForecastRule
{
    public List<string> Check(GlobalOccupancyForecast item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalOccupancyForecastCodeLengthRule(int MaxLength) : IGlobalOccupancyForecastRule
{
    public List<string> Check(GlobalOccupancyForecast item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global occupancy forecast.</summary>
public sealed record GlobalOccupancyForecastValidator(List<IGlobalOccupancyForecastRule> Rules)
{
    public List<string> Validate(GlobalOccupancyForecast item)
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

    public void RequireValid(GlobalOccupancyForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalOccupancyForecast> items)
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

/// <summary>Functions of global occupancy forecast rules.</summary>
public static class GlobalOccupancyForecastRules
{
    public static GlobalOccupancyForecastValidator DefaultGlobalOccupancyForecastValidator()
    {
        return new GlobalOccupancyForecastValidator(
            new List<IGlobalOccupancyForecastRule>
            {
                new GlobalOccupancyForecastActiveGuardRule(8.7),
                new GlobalOccupancyForecastLabelLengthRule(32),
                new GlobalOccupancyForecastCodePrefixRule("OPS"),
                new GlobalOccupancyForecastCodeLengthRule(86400),
            });
    }
}
