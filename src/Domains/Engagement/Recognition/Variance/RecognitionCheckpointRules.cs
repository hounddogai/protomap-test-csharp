namespace Acme.Workforce.Engagement.Recognition.Variance;

/// <summary>A rule a recognition checkpoint must pass. Check returns its problems, or none.</summary>
public interface IRecognitionCheckpointRule
{
    List<string> Check(RecognitionCheckpoint item);
}

public sealed record RecognitionCheckpointRequiredCodeRule(string Message) : IRecognitionCheckpointRule
{
    public List<string> Check(RecognitionCheckpoint item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RecognitionCheckpointStartsAtRangeRule(int Min, int Max) : IRecognitionCheckpointRule
{
    public List<string> Check(RecognitionCheckpoint item)
    {
        var valueValue = item.StartsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"starts at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"starts at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RecognitionCheckpointTeamLengthRule(int MaxLength) : IRecognitionCheckpointRule
{
    public List<string> Check(RecognitionCheckpoint item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionCheckpointRateRangeRule(double Min, double Max) : IRecognitionCheckpointRule
{
    public List<string> Check(RecognitionCheckpoint item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a recognition checkpoint.</summary>
public sealed record RecognitionCheckpointValidator(List<IRecognitionCheckpointRule> Rules)
{
    public List<string> Validate(RecognitionCheckpoint item)
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

    public void RequireValid(RecognitionCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RecognitionCheckpoint> items)
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

/// <summary>Functions of recognition checkpoint rules.</summary>
public static class RecognitionCheckpointRules
{
    public static RecognitionCheckpointValidator DefaultRecognitionCheckpointValidator()
    {
        return new RecognitionCheckpointValidator(
            new List<IRecognitionCheckpointRule>
            {
                new RecognitionCheckpointRequiredCodeRule("recognition checkpoint needs a value"),
                new RecognitionCheckpointStartsAtRangeRule(32, 255),
                new RecognitionCheckpointTeamLengthRule(1000),
                new RecognitionCheckpointRateRangeRule(1.9, 1842.6),
            });
    }
}
