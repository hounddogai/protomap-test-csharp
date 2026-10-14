namespace Acme.Workforce.Engagement.TownHall.Target;

/// <summary>A rule a seasonal town hall group must pass. Check returns its problems, or none.</summary>
public interface ISeasonalTownHallGroupRule
{
    List<string> Check(SeasonalTownHallGroup item);
}

public sealed record SeasonalTownHallGroupFloorValueRangeRule(double Min, double Max) : ISeasonalTownHallGroupRule
{
    public List<string> Check(SeasonalTownHallGroup item)
    {
        var valueValue = item.FloorValue;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"floor value {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"floor value {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SeasonalTownHallGroupNotesLengthRule(int MaxLength) : ISeasonalTownHallGroupRule
{
    public List<string> Check(SeasonalTownHallGroup item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalTownHallGroupUpdatedAtRangeRule(int Min, int Max) : ISeasonalTownHallGroupRule
{
    public List<string> Check(SeasonalTownHallGroup item)
    {
        var valueValue = item.UpdatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"updated at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalTownHallGroupMaximumRangeRule(double Min, double Max) : ISeasonalTownHallGroupRule
{
    public List<string> Check(SeasonalTownHallGroup item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalTownHallGroupReviewerLengthRule(int MaxLength) : ISeasonalTownHallGroupRule
{
    public List<string> Check(SeasonalTownHallGroup item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal town hall group.</summary>
public sealed record SeasonalTownHallGroupValidator(List<ISeasonalTownHallGroupRule> Rules)
{
    public List<string> Validate(SeasonalTownHallGroup item)
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

    public void RequireValid(SeasonalTownHallGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalTownHallGroup> items)
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

/// <summary>Functions of seasonal town hall group rules.</summary>
public static class SeasonalTownHallGroupRules
{
    public static SeasonalTownHallGroupValidator DefaultSeasonalTownHallGroupValidator()
    {
        return new SeasonalTownHallGroupValidator(
            new List<ISeasonalTownHallGroupRule>
            {
                new SeasonalTownHallGroupFloorValueRangeRule(1.3, 3934.6),
                new SeasonalTownHallGroupNotesLengthRule(32),
                new SeasonalTownHallGroupUpdatedAtRangeRule(80, 255),
                new SeasonalTownHallGroupMaximumRangeRule(2.1, 4637.7),
                new SeasonalTownHallGroupReviewerLengthRule(3600),
            });
    }
}
