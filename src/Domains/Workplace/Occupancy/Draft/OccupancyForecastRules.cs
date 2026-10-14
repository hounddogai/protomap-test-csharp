namespace Acme.Workforce.Workplace.Occupancy.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a occupancy forecast must pass. Check returns its problems, or none.</summary>
public interface IOccupancyForecastRule
{
    List<string> Check(OccupancyForecast item);
}

public sealed record OccupancyForecastMarginRangeRule(double Min, double Max) : IOccupancyForecastRule
{
    public List<string> Check(OccupancyForecast item)
    {
        var valueValue = item.Margin;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"margin {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyForecastCountryLengthRule(int MaxLength) : IOccupancyForecastRule
{
    public List<string> Check(OccupancyForecast item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyForecastRequiredTitleRule(string Message) : IOccupancyForecastRule
{
    public List<string> Check(OccupancyForecast item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OccupancyForecastAllowedTeamRule(List<string> Allowed) : IOccupancyForecastRule
{
    public List<string> Check(OccupancyForecast item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Team)))
        {
            return new List<string> { $"team {item.Team} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyForecastRequiredTeamRule(string Message) : IOccupancyForecastRule
{
    public List<string> Check(OccupancyForecast item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a occupancy forecast.</summary>
public sealed record OccupancyForecastValidator(List<IOccupancyForecastRule> Rules)
{
    public List<string> Validate(OccupancyForecast item)
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

    public void RequireValid(OccupancyForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OccupancyForecast> items)
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

/// <summary>Functions of occupancy forecast rules.</summary>
public static class OccupancyForecastRules
{
    public static OccupancyForecastValidator DefaultOccupancyForecastValidator()
    {
        return new OccupancyForecastValidator(
            new List<IOccupancyForecastRule>
            {
                new OccupancyForecastMarginRangeRule(2.1, 3492.9),
                new OccupancyForecastCountryLengthRule(16),
                new OccupancyForecastRequiredTitleRule("occupancy forecast needs an entry"),
                new OccupancyForecastAllowedTeamRule(new List<string> { "lenient", "manual", "regional" }),
                new OccupancyForecastRequiredTeamRule("occupancy forecast needs a value"),
            });
    }
}
