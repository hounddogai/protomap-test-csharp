namespace Acme.Workforce.Compliance.TrainingDue.Variance;

/// <summary>A rule a training due draft must pass. Check returns its problems, or none.</summary>
public interface ITrainingDueDraftRule
{
    List<string> Check(TrainingDueDraft item);
}

public sealed record TrainingDueDraftRatingRangeRule(double Min, double Max) : ITrainingDueDraftRule
{
    public List<string> Check(TrainingDueDraft item)
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

public sealed record TrainingDueDraftTagsLimitRule(int MaxCount, List<string> Banned) : ITrainingDueDraftRule
{
    public List<string> Check(TrainingDueDraft item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

public sealed record TrainingDueDraftManagerIdLengthRule(int MaxLength) : ITrainingDueDraftRule
{
    public List<string> Check(TrainingDueDraft item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueDraftBatchSizeRangeRule(int Min, int Max) : ITrainingDueDraftRule
{
    public List<string> Check(TrainingDueDraft item)
    {
        var valueValue = item.BatchSize;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"batch size {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a training due draft.</summary>
public sealed record TrainingDueDraftValidator(List<ITrainingDueDraftRule> Rules)
{
    public List<string> Validate(TrainingDueDraft item)
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

    public void RequireValid(TrainingDueDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TrainingDueDraft> items)
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

/// <summary>Functions of training due draft rules.</summary>
public static class TrainingDueDraftRules
{
    public static TrainingDueDraftValidator DefaultTrainingDueDraftValidator()
    {
        return new TrainingDueDraftValidator(
            new List<ITrainingDueDraftRule>
            {
                new TrainingDueDraftRatingRangeRule(2.0, 3099.7),
                new TrainingDueDraftTagsLimitRule(3600, new List<string> { "tiered", "manual", "capped" }),
                new TrainingDueDraftManagerIdLengthRule(32),
                new TrainingDueDraftBatchSizeRangeRule(255, 1000),
            });
    }
}
