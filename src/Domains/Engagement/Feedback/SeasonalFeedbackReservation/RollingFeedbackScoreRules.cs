namespace Acme.Workforce.Engagement.Feedback.SeasonalFeedbackReservation;

/// <summary>A rule a rolling feedback score must pass. Check returns its problems, or none.</summary>
public interface IRollingFeedbackScoreRule
{
    List<string> Check(RollingFeedbackScore item);
}

public sealed record RollingFeedbackScoreRequiredNameRule(string Message) : IRollingFeedbackScoreRule
{
    public List<string> Check(RollingFeedbackScore item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingFeedbackScoreChannelLengthRule(int MaxLength) : IRollingFeedbackScoreRule
{
    public List<string> Check(RollingFeedbackScore item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingFeedbackScoreCodeLengthRule(int MaxLength) : IRollingFeedbackScoreRule
{
    public List<string> Check(RollingFeedbackScore item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingFeedbackScoreSequenceRangeRule(int Min, int Max) : IRollingFeedbackScoreRule
{
    public List<string> Check(RollingFeedbackScore item)
    {
        var valueValue = item.Sequence;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"sequence {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"sequence {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RollingFeedbackScoreCodePrefixRule(string Prefix) : IRollingFeedbackScoreRule
{
    public List<string> Check(RollingFeedbackScore item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RollingFeedbackScoreRateRangeRule(double Min, double Max) : IRollingFeedbackScoreRule
{
    public List<string> Check(RollingFeedbackScore item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling feedback score.</summary>
public sealed record RollingFeedbackScoreValidator(List<IRollingFeedbackScoreRule> Rules)
{
    public List<string> Validate(RollingFeedbackScore item)
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

    public void RequireValid(RollingFeedbackScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingFeedbackScore> items)
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

/// <summary>Functions of rolling feedback score rules.</summary>
public static class RollingFeedbackScoreRules
{
    public static RollingFeedbackScoreValidator DefaultRollingFeedbackScoreValidator()
    {
        return new RollingFeedbackScoreValidator(
            new List<IRollingFeedbackScoreRule>
            {
                new RollingFeedbackScoreRequiredNameRule("rolling feedback score needs a value"),
                new RollingFeedbackScoreChannelLengthRule(80),
                new RollingFeedbackScoreCodeLengthRule(32),
                new RollingFeedbackScoreSequenceRangeRule(1000, 1000),
                new RollingFeedbackScoreCodePrefixRule("REF"),
                new RollingFeedbackScoreRateRangeRule(0.8, 1489.6),
            });
    }
}
