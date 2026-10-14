namespace Acme.Workforce.Compliance.TrainingDue.Mapping;

/// <summary>A rule a training due score must pass. Check returns its problems, or none.</summary>
public interface ITrainingDueScoreRule
{
    List<string> Check(TrainingDueScore item);
}

public sealed record TrainingDueScoreLeaveDaysRangeRule(int Min, int Max) : ITrainingDueScoreRule
{
    public List<string> Check(TrainingDueScore item)
    {
        var valueValue = item.LeaveDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"leave days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"leave days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TrainingDueScoreEndsAtRangeRule(int Min, int Max) : ITrainingDueScoreRule
{
    public List<string> Check(TrainingDueScore item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueScoreCapacityRangeRule(int Min, int Max) : ITrainingDueScoreRule
{
    public List<string> Check(TrainingDueScore item)
    {
        var valueValue = item.Capacity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"capacity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueScoreRequiredCodeRule(string Message) : ITrainingDueScoreRule
{
    public List<string> Check(TrainingDueScore item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a training due score.</summary>
public sealed record TrainingDueScoreValidator(List<ITrainingDueScoreRule> Rules)
{
    public List<string> Validate(TrainingDueScore item)
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

    public void RequireValid(TrainingDueScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TrainingDueScore> items)
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

/// <summary>Functions of training due score rules.</summary>
public static class TrainingDueScoreRules
{
    public static TrainingDueScoreValidator DefaultTrainingDueScoreValidator()
    {
        return new TrainingDueScoreValidator(
            new List<ITrainingDueScoreRule>
            {
                new TrainingDueScoreLeaveDaysRangeRule(16, 3600),
                new TrainingDueScoreEndsAtRangeRule(120, 86400),
                new TrainingDueScoreCapacityRangeRule(16, 86400),
                new TrainingDueScoreRequiredCodeRule("training due score needs this field"),
            });
    }
}
