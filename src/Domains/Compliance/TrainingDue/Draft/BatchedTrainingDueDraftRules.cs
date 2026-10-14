namespace Acme.Workforce.Compliance.TrainingDue.Draft;

/// <summary>A rule a batched training due draft must pass. Check returns its problems, or none.</summary>
public interface IBatchedTrainingDueDraftRule
{
    List<string> Check(BatchedTrainingDueDraft item);
}

public sealed record BatchedTrainingDueDraftRequiredTitleRule(string Message) : IBatchedTrainingDueDraftRule
{
    public List<string> Check(BatchedTrainingDueDraft item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedTrainingDueDraftRequiredSkillRule(string Message) : IBatchedTrainingDueDraftRule
{
    public List<string> Check(BatchedTrainingDueDraft item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedTrainingDueDraftRatioRangeRule(double Min, double Max) : IBatchedTrainingDueDraftRule
{
    public List<string> Check(BatchedTrainingDueDraft item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched training due draft.</summary>
public sealed record BatchedTrainingDueDraftValidator(List<IBatchedTrainingDueDraftRule> Rules)
{
    public List<string> Validate(BatchedTrainingDueDraft item)
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

    public void RequireValid(BatchedTrainingDueDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedTrainingDueDraft> items)
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

/// <summary>Functions of batched training due draft rules.</summary>
public static class BatchedTrainingDueDraftRules
{
    public static BatchedTrainingDueDraftValidator DefaultBatchedTrainingDueDraftValidator()
    {
        return new BatchedTrainingDueDraftValidator(
            new List<IBatchedTrainingDueDraftRule>
            {
                new BatchedTrainingDueDraftRequiredTitleRule("batched training due draft needs an entry"),
                new BatchedTrainingDueDraftRequiredSkillRule("batched training due draft needs a value"),
                new BatchedTrainingDueDraftRatioRangeRule(5.0, 4238.4),
            });
    }
}
