namespace Acme.Workforce.Engagement.Recognition.RecognitionVariance;

/// <summary>A rule a manual recognition schedule must pass. Check returns its problems, or none.</summary>
public interface IManualRecognitionScheduleRule
{
    List<string> Check(ManualRecognitionSchedule item);
}

public sealed record ManualRecognitionScheduleRequiredLocationRule(string Message) : IManualRecognitionScheduleRule
{
    public List<string> Check(ManualRecognitionSchedule item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualRecognitionScheduleCodePrefixRule(string Prefix) : IManualRecognitionScheduleRule
{
    public List<string> Check(ManualRecognitionSchedule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ManualRecognitionScheduleRequiredCodeRule(string Message) : IManualRecognitionScheduleRule
{
    public List<string> Check(ManualRecognitionSchedule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualRecognitionScheduleRequiredTitleRule(string Message) : IManualRecognitionScheduleRule
{
    public List<string> Check(ManualRecognitionSchedule item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualRecognitionScheduleRatingRangeRule(double Min, double Max) : IManualRecognitionScheduleRule
{
    public List<string> Check(ManualRecognitionSchedule item)
    {
        var valueValue = item.Rating;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rating {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rating {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ManualRecognitionScheduleCodeLengthRule(int MaxLength) : IManualRecognitionScheduleRule
{
    public List<string> Check(ManualRecognitionSchedule item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual recognition schedule.</summary>
public sealed record ManualRecognitionScheduleValidator(List<IManualRecognitionScheduleRule> Rules)
{
    public List<string> Validate(ManualRecognitionSchedule item)
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

    public void RequireValid(ManualRecognitionSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualRecognitionSchedule> items)
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

/// <summary>Functions of manual recognition schedule rules.</summary>
public static class ManualRecognitionScheduleRules
{
    public static ManualRecognitionScheduleValidator DefaultManualRecognitionScheduleValidator()
    {
        return new ManualRecognitionScheduleValidator(
            new List<IManualRecognitionScheduleRule>
            {
                new ManualRecognitionScheduleRequiredLocationRule("manual recognition schedule needs this field"),
                new ManualRecognitionScheduleCodePrefixRule("X"),
                new ManualRecognitionScheduleRequiredCodeRule("manual recognition schedule needs a value"),
                new ManualRecognitionScheduleRequiredTitleRule("manual recognition schedule needs this field"),
                new ManualRecognitionScheduleRatingRangeRule(1.5, 2620.5),
                new ManualRecognitionScheduleCodeLengthRule(86400),
            });
    }
}
