namespace Acme.Workforce.Engagement.TownHall.Review;

/// <summary>A rule a town hall forecast must pass. Check returns its problems, or none.</summary>
public interface ITownHallForecastRule
{
    List<string> Check(TownHallForecast item);
}

public sealed record TownHallForecastCodeLengthRule(int MaxLength) : ITownHallForecastRule
{
    public List<string> Check(TownHallForecast item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallForecastChecksumLengthRule(int MaxLength) : ITownHallForecastRule
{
    public List<string> Check(TownHallForecast item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallForecastRequiredChecksumRule(string Message) : ITownHallForecastRule
{
    public List<string> Check(TownHallForecast item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a town hall forecast.</summary>
public sealed record TownHallForecastValidator(List<ITownHallForecastRule> Rules)
{
    public List<string> Validate(TownHallForecast item)
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

    public void RequireValid(TownHallForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TownHallForecast> items)
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

/// <summary>Functions of town hall forecast rules.</summary>
public static class TownHallForecastRules
{
    public static TownHallForecastValidator DefaultTownHallForecastValidator()
    {
        return new TownHallForecastValidator(
            new List<ITownHallForecastRule>
            {
                new TownHallForecastCodeLengthRule(120),
                new TownHallForecastChecksumLengthRule(16),
                new TownHallForecastRequiredChecksumRule("town hall forecast needs a value"),
            });
    }
}
