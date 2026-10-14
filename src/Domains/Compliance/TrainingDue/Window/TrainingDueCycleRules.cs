namespace Acme.Workforce.Compliance.TrainingDue.Window;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a training due cycle must pass. Check returns its problems, or none.</summary>
public interface ITrainingDueCycleRule
{
    List<string> Check(TrainingDueCycle item);
}

public sealed record TrainingDueCycleRequiredNameRule(string Message) : ITrainingDueCycleRule
{
    public List<string> Check(TrainingDueCycle item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueCycleCreatedAtRangeRule(int Min, int Max) : ITrainingDueCycleRule
{
    public List<string> Check(TrainingDueCycle item)
    {
        var valueValue = item.CreatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"created at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueCycleTitleLengthRule(int MaxLength) : ITrainingDueCycleRule
{
    public List<string> Check(TrainingDueCycle item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueCycleRequiredTitleRule(string Message) : ITrainingDueCycleRule
{
    public List<string> Check(TrainingDueCycle item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueCycleAllowedNameRule(List<string> Allowed) : ITrainingDueCycleRule
{
    public List<string> Check(TrainingDueCycle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is above maximum" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a training due cycle.</summary>
public sealed record TrainingDueCycleValidator(List<ITrainingDueCycleRule> Rules)
{
    public List<string> Validate(TrainingDueCycle item)
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

    public void RequireValid(TrainingDueCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TrainingDueCycle> items)
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

/// <summary>Functions of training due cycle rules.</summary>
public static class TrainingDueCycleRules
{
    public static TrainingDueCycleValidator DefaultTrainingDueCycleValidator()
    {
        return new TrainingDueCycleValidator(
            new List<ITrainingDueCycleRule>
            {
                new TrainingDueCycleRequiredNameRule("training due cycle needs an entry"),
                new TrainingDueCycleCreatedAtRangeRule(120, 255),
                new TrainingDueCycleTitleLengthRule(64),
                new TrainingDueCycleRequiredTitleRule("training due cycle needs this field"),
                new TrainingDueCycleAllowedNameRule(new List<string> { "weighted", "manual", "weighted" }),
            });
    }
}
