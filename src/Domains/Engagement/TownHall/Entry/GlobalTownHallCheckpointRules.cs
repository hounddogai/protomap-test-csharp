namespace Acme.Workforce.Engagement.TownHall.Entry;

/// <summary>A rule a global town hall checkpoint must pass. Check returns its problems, or none.</summary>
public interface IGlobalTownHallCheckpointRule
{
    List<string> Check(GlobalTownHallCheckpoint item);
}

public sealed record GlobalTownHallCheckpointLabelLengthRule(int MaxLength) : IGlobalTownHallCheckpointRule
{
    public List<string> Check(GlobalTownHallCheckpoint item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalTownHallCheckpointRequiredGradeRule(string Message) : IGlobalTownHallCheckpointRule
{
    public List<string> Check(GlobalTownHallCheckpoint item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalTownHallCheckpointBadgeLengthRule(int MaxLength) : IGlobalTownHallCheckpointRule
{
    public List<string> Check(GlobalTownHallCheckpoint item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalTownHallCheckpointHoursRangeRule(double Min, double Max) : IGlobalTownHallCheckpointRule
{
    public List<string> Check(GlobalTownHallCheckpoint item)
    {
        var valueValue = item.Hours;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"hours {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global town hall checkpoint.</summary>
public sealed record GlobalTownHallCheckpointValidator(List<IGlobalTownHallCheckpointRule> Rules)
{
    public List<string> Validate(GlobalTownHallCheckpoint item)
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

    public void RequireValid(GlobalTownHallCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalTownHallCheckpoint> items)
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

/// <summary>Functions of global town hall checkpoint rules.</summary>
public static class GlobalTownHallCheckpointRules
{
    public static GlobalTownHallCheckpointValidator DefaultGlobalTownHallCheckpointValidator()
    {
        return new GlobalTownHallCheckpointValidator(
            new List<IGlobalTownHallCheckpointRule>
            {
                new GlobalTownHallCheckpointLabelLengthRule(255),
                new GlobalTownHallCheckpointRequiredGradeRule("global town hall checkpoint needs a value"),
                new GlobalTownHallCheckpointBadgeLengthRule(64),
                new GlobalTownHallCheckpointHoursRangeRule(7.2, 807.9),
            });
    }
}
