namespace Acme.Workforce.Engagement.Recognition.Plan;

/// <summary>A rule a pending recognition batch must pass. Check returns its problems, or none.</summary>
public interface IPendingRecognitionBatchRule
{
    List<string> Check(PendingRecognitionBatch item);
}

public sealed record PendingRecognitionBatchTitleLengthRule(int MaxLength) : IPendingRecognitionBatchRule
{
    public List<string> Check(PendingRecognitionBatch item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingRecognitionBatchChannelLengthRule(int MaxLength) : IPendingRecognitionBatchRule
{
    public List<string> Check(PendingRecognitionBatch item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingRecognitionBatchRatioRangeRule(double Min, double Max) : IPendingRecognitionBatchRule
{
    public List<string> Check(PendingRecognitionBatch item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PendingRecognitionBatchDurationMinutesRangeRule(int Min, int Max) : IPendingRecognitionBatchRule
{
    public List<string> Check(PendingRecognitionBatch item)
    {
        var valueValue = item.DurationMinutes;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"duration minutes {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PendingRecognitionBatchRequiredChannelRule(string Message) : IPendingRecognitionBatchRule
{
    public List<string> Check(PendingRecognitionBatch item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingRecognitionBatchRequiredCodeRule(string Message) : IPendingRecognitionBatchRule
{
    public List<string> Check(PendingRecognitionBatch item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending recognition batch.</summary>
public sealed record PendingRecognitionBatchValidator(List<IPendingRecognitionBatchRule> Rules)
{
    public List<string> Validate(PendingRecognitionBatch item)
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

    public void RequireValid(PendingRecognitionBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingRecognitionBatch> items)
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

/// <summary>Functions of pending recognition batch rules.</summary>
public static class PendingRecognitionBatchRules
{
    public static PendingRecognitionBatchValidator DefaultPendingRecognitionBatchValidator()
    {
        return new PendingRecognitionBatchValidator(
            new List<IPendingRecognitionBatchRule>
            {
                new PendingRecognitionBatchTitleLengthRule(32),
                new PendingRecognitionBatchChannelLengthRule(80),
                new PendingRecognitionBatchRatioRangeRule(3.5, 1931.5),
                new PendingRecognitionBatchDurationMinutesRangeRule(16, 32),
                new PendingRecognitionBatchRequiredChannelRule("pending recognition batch needs an entry"),
                new PendingRecognitionBatchRequiredCodeRule("pending recognition batch needs a value"),
            });
    }
}
