namespace Acme.Workforce.Compliance.TrainingDue.Notice;

/// <summary>A rule a training due notice must pass. Check returns its problems, or none.</summary>
public interface ITrainingDueNoticeRule
{
    List<string> Check(TrainingDueNotice item);
}

public sealed record TrainingDueNoticeCodePrefixRule(string Prefix) : ITrainingDueNoticeRule
{
    public List<string> Check(TrainingDueNotice item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueNoticeLabelLengthRule(int MaxLength) : ITrainingDueNoticeRule
{
    public List<string> Check(TrainingDueNotice item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueNoticeAttemptsRangeRule(int Min, int Max) : ITrainingDueNoticeRule
{
    public List<string> Check(TrainingDueNotice item)
    {
        var valueValue = item.Attempts;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"attempts {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"attempts {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TrainingDueNoticeThresholdRangeRule(double Min, double Max) : ITrainingDueNoticeRule
{
    public List<string> Check(TrainingDueNotice item)
    {
        var valueValue = item.Threshold;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"threshold {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"threshold {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a training due notice.</summary>
public sealed record TrainingDueNoticeValidator(List<ITrainingDueNoticeRule> Rules)
{
    public List<string> Validate(TrainingDueNotice item)
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

    public void RequireValid(TrainingDueNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TrainingDueNotice> items)
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

/// <summary>Functions of training due notice rules.</summary>
public static class TrainingDueNoticeRules
{
    public static TrainingDueNoticeValidator DefaultTrainingDueNoticeValidator()
    {
        return new TrainingDueNoticeValidator(
            new List<ITrainingDueNoticeRule>
            {
                new TrainingDueNoticeCodePrefixRule("X"),
                new TrainingDueNoticeLabelLengthRule(16),
                new TrainingDueNoticeAttemptsRangeRule(80, 86400),
                new TrainingDueNoticeThresholdRangeRule(0.2, 1139.6),
            });
    }
}
