namespace Acme.Workforce.Workplace.Occupancy.Checkpoint;

/// <summary>A rule a weighted occupancy forecast must pass. Check returns its problems, or none.</summary>
public interface IWeightedOccupancyForecastRule
{
    List<string> Check(WeightedOccupancyForecast item);
}

public sealed record WeightedOccupancyForecastRatioRangeRule(double Min, double Max) : IWeightedOccupancyForecastRule
{
    public List<string> Check(WeightedOccupancyForecast item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedOccupancyForecastRequiredCodeRule(string Message) : IWeightedOccupancyForecastRule
{
    public List<string> Check(WeightedOccupancyForecast item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedOccupancyForecastPinnedGuardRule(int Limit) : IWeightedOccupancyForecastRule
{
    public List<string> Check(WeightedOccupancyForecast item)
    {
        if (item.Pinned && item.UpdatedAt > Limit)
        {
            return new List<string> { $"updated at {item.UpdatedAt} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record WeightedOccupancyForecastSkillLengthRule(int MaxLength) : IWeightedOccupancyForecastRule
{
    public List<string> Check(WeightedOccupancyForecast item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted occupancy forecast.</summary>
public sealed record WeightedOccupancyForecastValidator(List<IWeightedOccupancyForecastRule> Rules)
{
    public List<string> Validate(WeightedOccupancyForecast item)
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

    public void RequireValid(WeightedOccupancyForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedOccupancyForecast> items)
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

/// <summary>Functions of weighted occupancy forecast rules.</summary>
public static class WeightedOccupancyForecastRules
{
    public static WeightedOccupancyForecastValidator DefaultWeightedOccupancyForecastValidator()
    {
        return new WeightedOccupancyForecastValidator(
            new List<IWeightedOccupancyForecastRule>
            {
                new WeightedOccupancyForecastRatioRangeRule(6.2, 4933.5),
                new WeightedOccupancyForecastRequiredCodeRule("weighted occupancy forecast needs this field"),
                new WeightedOccupancyForecastPinnedGuardRule(86400),
                new WeightedOccupancyForecastSkillLengthRule(3600),
            });
    }
}
