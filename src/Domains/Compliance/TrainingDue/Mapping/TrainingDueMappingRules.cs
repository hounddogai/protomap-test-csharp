namespace Acme.Workforce.Compliance.TrainingDue.Mapping;

/// <summary>A rule a training due mapping must pass. Check returns its problems, or none.</summary>
public interface ITrainingDueMappingRule
{
    List<string> Check(TrainingDueMapping item);
}

public sealed record TrainingDueMappingBadgeLengthRule(int MaxLength) : ITrainingDueMappingRule
{
    public List<string> Check(TrainingDueMapping item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueMappingRequiredSegmentRule(string Message) : ITrainingDueMappingRule
{
    public List<string> Check(TrainingDueMapping item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueMappingNameLengthRule(int MaxLength) : ITrainingDueMappingRule
{
    public List<string> Check(TrainingDueMapping item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a training due mapping.</summary>
public sealed record TrainingDueMappingValidator(List<ITrainingDueMappingRule> Rules)
{
    public List<string> Validate(TrainingDueMapping item)
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

    public void RequireValid(TrainingDueMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TrainingDueMapping> items)
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

/// <summary>Functions of training due mapping rules.</summary>
public static class TrainingDueMappingRules
{
    public static TrainingDueMappingValidator DefaultTrainingDueMappingValidator()
    {
        return new TrainingDueMappingValidator(
            new List<ITrainingDueMappingRule>
            {
                new TrainingDueMappingBadgeLengthRule(16),
                new TrainingDueMappingRequiredSegmentRule("training due mapping needs an entry"),
                new TrainingDueMappingNameLengthRule(86400),
            });
    }
}
