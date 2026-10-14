namespace Acme.Workforce.Compliance.TrainingDue.Notice;

/// <summary>A rule a training due audit must pass. Check returns its problems, or none.</summary>
public interface ITrainingDueAuditRule
{
    List<string> Check(TrainingDueAudit item);
}

public sealed record TrainingDueAuditBatchSizeRangeRule(int Min, int Max) : ITrainingDueAuditRule
{
    public List<string> Check(TrainingDueAudit item)
    {
        var valueValue = item.BatchSize;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"batch size {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"batch size {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TrainingDueAuditRequiredBadgeRule(string Message) : ITrainingDueAuditRule
{
    public List<string> Check(TrainingDueAudit item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueAuditTitleLengthRule(int MaxLength) : ITrainingDueAuditRule
{
    public List<string> Check(TrainingDueAudit item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a training due audit.</summary>
public sealed record TrainingDueAuditValidator(List<ITrainingDueAuditRule> Rules)
{
    public List<string> Validate(TrainingDueAudit item)
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

    public void RequireValid(TrainingDueAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TrainingDueAudit> items)
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

/// <summary>Functions of training due audit rules.</summary>
public static class TrainingDueAuditRules
{
    public static TrainingDueAuditValidator DefaultTrainingDueAuditValidator()
    {
        return new TrainingDueAuditValidator(
            new List<ITrainingDueAuditRule>
            {
                new TrainingDueAuditBatchSizeRangeRule(64, 86400),
                new TrainingDueAuditRequiredBadgeRule("training due audit needs a value"),
                new TrainingDueAuditTitleLengthRule(16),
            });
    }
}
