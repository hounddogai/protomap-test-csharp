namespace Acme.Workforce.Engagement.TownHall.Milestone;

/// <summary>A rule a town hall metric must pass. Check returns its problems, or none.</summary>
public interface ITownHallMetricRule
{
    List<string> Check(TownHallMetric item);
}

public sealed record TownHallMetricLocationLengthRule(int MaxLength) : ITownHallMetricRule
{
    public List<string> Check(TownHallMetric item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallMetricCodeLengthRule(int MaxLength) : ITownHallMetricRule
{
    public List<string> Check(TownHallMetric item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallMetricScoreRangeRule(double Min, double Max) : ITownHallMetricRule
{
    public List<string> Check(TownHallMetric item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a town hall metric.</summary>
public sealed record TownHallMetricValidator(List<ITownHallMetricRule> Rules)
{
    public List<string> Validate(TownHallMetric item)
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

    public void RequireValid(TownHallMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TownHallMetric> items)
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

/// <summary>Functions of town hall metric rules.</summary>
public static class TownHallMetricRules
{
    public static TownHallMetricValidator DefaultTownHallMetricValidator()
    {
        return new TownHallMetricValidator(
            new List<ITownHallMetricRule>
            {
                new TownHallMetricLocationLengthRule(16),
                new TownHallMetricCodeLengthRule(120),
                new TownHallMetricScoreRangeRule(7.4, 5004.3),
            });
    }
}
