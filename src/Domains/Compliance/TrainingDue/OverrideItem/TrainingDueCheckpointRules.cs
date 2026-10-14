namespace Acme.Workforce.Compliance.TrainingDue.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a training due checkpoint must pass. Check returns its problems, or none.</summary>
public interface ITrainingDueCheckpointRule
{
    List<string> Check(TrainingDueCheckpoint item);
}

public sealed record TrainingDueCheckpointRequiredSegmentRule(string Message) : ITrainingDueCheckpointRule
{
    public List<string> Check(TrainingDueCheckpoint item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueCheckpointAllowedCodeRule(List<string> Allowed) : ITrainingDueCheckpointRule
{
    public List<string> Check(TrainingDueCheckpoint item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueCheckpointExpiresAtRangeRule(int Min, int Max) : ITrainingDueCheckpointRule
{
    public List<string> Check(TrainingDueCheckpoint item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueCheckpointSequenceRangeRule(int Min, int Max) : ITrainingDueCheckpointRule
{
    public List<string> Check(TrainingDueCheckpoint item)
    {
        var valueValue = item.Sequence;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"sequence {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a training due checkpoint.</summary>
public sealed record TrainingDueCheckpointValidator(List<ITrainingDueCheckpointRule> Rules)
{
    public List<string> Validate(TrainingDueCheckpoint item)
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

    public void RequireValid(TrainingDueCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TrainingDueCheckpoint> items)
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

/// <summary>Functions of training due checkpoint rules.</summary>
public static class TrainingDueCheckpointRules
{
    public static TrainingDueCheckpointValidator DefaultTrainingDueCheckpointValidator()
    {
        return new TrainingDueCheckpointValidator(
            new List<ITrainingDueCheckpointRule>
            {
                new TrainingDueCheckpointRequiredSegmentRule("training due checkpoint needs this field"),
                new TrainingDueCheckpointAllowedCodeRule(new List<string> { "seasonal", "capped" }),
                new TrainingDueCheckpointExpiresAtRangeRule(1000, 3600),
                new TrainingDueCheckpointSequenceRangeRule(16, 3600),
            });
    }
}
