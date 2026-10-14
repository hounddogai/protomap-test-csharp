namespace Acme.Workforce.Compliance.TrainingDue.Window;

/// <summary>A rule a training due window must pass. Check returns its problems, or none.</summary>
public interface ITrainingDueWindowRule
{
    List<string> Check(TrainingDueWindow item);
}

public sealed record TrainingDueWindowRequiredTitleRule(string Message) : ITrainingDueWindowRule
{
    public List<string> Check(TrainingDueWindow item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueWindowVersionRangeRule(int Min, int Max) : ITrainingDueWindowRule
{
    public List<string> Check(TrainingDueWindow item)
    {
        var valueValue = item.Version;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"version {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"version {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TrainingDueWindowChannelLengthRule(int MaxLength) : ITrainingDueWindowRule
{
    public List<string> Check(TrainingDueWindow item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a training due window.</summary>
public sealed record TrainingDueWindowValidator(List<ITrainingDueWindowRule> Rules)
{
    public List<string> Validate(TrainingDueWindow item)
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

    public void RequireValid(TrainingDueWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TrainingDueWindow> items)
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

/// <summary>Functions of training due window rules.</summary>
public static class TrainingDueWindowRules
{
    public static TrainingDueWindowValidator DefaultTrainingDueWindowValidator()
    {
        return new TrainingDueWindowValidator(
            new List<ITrainingDueWindowRule>
            {
                new TrainingDueWindowRequiredTitleRule("training due window needs an entry"),
                new TrainingDueWindowVersionRangeRule(255, 255),
                new TrainingDueWindowChannelLengthRule(1000),
            });
    }
}
