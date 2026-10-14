namespace Acme.Workforce.Engagement.Recognition.Score;

/// <summary>A rule a recognition score must pass. Check returns its problems, or none.</summary>
public interface IRecognitionScoreRule
{
    List<string> Check(RecognitionScore item);
}

public sealed record RecognitionScoreDurationMinutesRangeRule(int Min, int Max) : IRecognitionScoreRule
{
    public List<string> Check(RecognitionScore item)
    {
        var valueValue = item.DurationMinutes;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"duration minutes {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"duration minutes {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RecognitionScoreCodeLengthRule(int MaxLength) : IRecognitionScoreRule
{
    public List<string> Check(RecognitionScore item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionScoreRequiredCodeRule(string Message) : IRecognitionScoreRule
{
    public List<string> Check(RecognitionScore item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RecognitionScoreEmployeeIdLengthRule(int MaxLength) : IRecognitionScoreRule
{
    public List<string> Check(RecognitionScore item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a recognition score.</summary>
public sealed record RecognitionScoreValidator(List<IRecognitionScoreRule> Rules)
{
    public List<string> Validate(RecognitionScore item)
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

    public void RequireValid(RecognitionScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RecognitionScore> items)
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

/// <summary>Functions of recognition score rules.</summary>
public static class RecognitionScoreRules
{
    public static RecognitionScoreValidator DefaultRecognitionScoreValidator()
    {
        return new RecognitionScoreValidator(
            new List<IRecognitionScoreRule>
            {
                new RecognitionScoreDurationMinutesRangeRule(32, 255),
                new RecognitionScoreCodeLengthRule(1000),
                new RecognitionScoreRequiredCodeRule("recognition score needs an entry"),
                new RecognitionScoreEmployeeIdLengthRule(16),
            });
    }
}
