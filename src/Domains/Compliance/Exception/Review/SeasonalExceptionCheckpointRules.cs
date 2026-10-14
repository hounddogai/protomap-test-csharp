namespace Acme.Workforce.Compliance.Exception.Review;

/// <summary>A rule a seasonal exception checkpoint must pass. Check returns its problems, or none.</summary>
public interface ISeasonalExceptionCheckpointRule
{
    List<string> Check(SeasonalExceptionCheckpoint item);
}

public sealed record SeasonalExceptionCheckpointRequiredLabelRule(string Message) : ISeasonalExceptionCheckpointRule
{
    public List<string> Check(SeasonalExceptionCheckpoint item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalExceptionCheckpointDurationMinutesRangeRule(int Min, int Max) : ISeasonalExceptionCheckpointRule
{
    public List<string> Check(SeasonalExceptionCheckpoint item)
    {
        var valueValue = item.DurationMinutes;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"duration minutes {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalExceptionCheckpointDescriptionLengthRule(int MaxLength) : ISeasonalExceptionCheckpointRule
{
    public List<string> Check(SeasonalExceptionCheckpoint item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal exception checkpoint.</summary>
public sealed record SeasonalExceptionCheckpointValidator(List<ISeasonalExceptionCheckpointRule> Rules)
{
    public List<string> Validate(SeasonalExceptionCheckpoint item)
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

    public void RequireValid(SeasonalExceptionCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalExceptionCheckpoint> items)
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

/// <summary>Functions of seasonal exception checkpoint rules.</summary>
public static class SeasonalExceptionCheckpointRules
{
    public static SeasonalExceptionCheckpointValidator DefaultSeasonalExceptionCheckpointValidator()
    {
        return new SeasonalExceptionCheckpointValidator(
            new List<ISeasonalExceptionCheckpointRule>
            {
                new SeasonalExceptionCheckpointRequiredLabelRule("seasonal exception checkpoint needs this field"),
                new SeasonalExceptionCheckpointDurationMinutesRangeRule(120, 86400),
                new SeasonalExceptionCheckpointDescriptionLengthRule(86400),
            });
    }
}
