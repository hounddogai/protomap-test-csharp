namespace Acme.Workforce.Engagement.Recognition.Schedule;

/// <summary>A rule a recognition schedule must pass. Check returns its problems, or none.</summary>
public interface IRecognitionScheduleRule
{
    List<string> Check(RecognitionSchedule item);
}

public sealed record RecognitionScheduleCodePrefixRule(string Prefix) : IRecognitionScheduleRule
{
    public List<string> Check(RecognitionSchedule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionScheduleRequiredTitleRule(string Message) : IRecognitionScheduleRule
{
    public List<string> Check(RecognitionSchedule item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RecognitionScheduleGraceDaysRangeRule(int Min, int Max) : IRecognitionScheduleRule
{
    public List<string> Check(RecognitionSchedule item)
    {
        var valueValue = item.GraceDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"grace days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"grace days {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a recognition schedule.</summary>
public sealed record RecognitionScheduleValidator(List<IRecognitionScheduleRule> Rules)
{
    public List<string> Validate(RecognitionSchedule item)
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

    public void RequireValid(RecognitionSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RecognitionSchedule> items)
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

/// <summary>Functions of recognition schedule rules.</summary>
public static class RecognitionScheduleRules
{
    public static RecognitionScheduleValidator DefaultRecognitionScheduleValidator()
    {
        return new RecognitionScheduleValidator(
            new List<IRecognitionScheduleRule>
            {
                new RecognitionScheduleCodePrefixRule("X"),
                new RecognitionScheduleRequiredTitleRule("recognition schedule needs this field"),
                new RecognitionScheduleGraceDaysRangeRule(64, 3600),
            });
    }
}
