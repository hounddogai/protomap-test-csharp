namespace Acme.Workforce.Engagement.TownHall.Draft;

/// <summary>A rule a town hall bundle must pass. Check returns its problems, or none.</summary>
public interface ITownHallBundleRule
{
    List<string> Check(TownHallBundle item);
}

public sealed record TownHallBundleSegmentLengthRule(int MaxLength) : ITownHallBundleRule
{
    public List<string> Check(TownHallBundle item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallBundleCodeLengthRule(int MaxLength) : ITownHallBundleRule
{
    public List<string> Check(TownHallBundle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallBundleScoreRangeRule(double Min, double Max) : ITownHallBundleRule
{
    public List<string> Check(TownHallBundle item)
    {
        var valueValue = item.Score;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"score {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"score {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a town hall bundle.</summary>
public sealed record TownHallBundleValidator(List<ITownHallBundleRule> Rules)
{
    public List<string> Validate(TownHallBundle item)
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

    public void RequireValid(TownHallBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TownHallBundle> items)
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

/// <summary>Functions of town hall bundle rules.</summary>
public static class TownHallBundleRules
{
    public static TownHallBundleValidator DefaultTownHallBundleValidator()
    {
        return new TownHallBundleValidator(
            new List<ITownHallBundleRule>
            {
                new TownHallBundleSegmentLengthRule(3600),
                new TownHallBundleCodeLengthRule(255),
                new TownHallBundleScoreRangeRule(2.2, 3658.4),
            });
    }
}
