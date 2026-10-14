namespace Acme.Workforce.Compliance.TrainingDue.Variance;

/// <summary>A rule a training due variance must pass. Check returns its problems, or none.</summary>
public interface ITrainingDueVarianceRule
{
    List<string> Check(TrainingDueVariance item);
}

public sealed record TrainingDueVarianceRequiredCodeRule(string Message) : ITrainingDueVarianceRule
{
    public List<string> Check(TrainingDueVariance item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueVarianceNameLengthRule(int MaxLength) : ITrainingDueVarianceRule
{
    public List<string> Check(TrainingDueVariance item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueVarianceVersionRangeRule(int Min, int Max) : ITrainingDueVarianceRule
{
    public List<string> Check(TrainingDueVariance item)
    {
        var valueValue = item.Version;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"version {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueVarianceRequiredGradeRule(string Message) : ITrainingDueVarianceRule
{
    public List<string> Check(TrainingDueVariance item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueVarianceThresholdRangeRule(double Min, double Max) : ITrainingDueVarianceRule
{
    public List<string> Check(TrainingDueVariance item)
    {
        var valueValue = item.Threshold;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"threshold {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a training due variance.</summary>
public sealed record TrainingDueVarianceValidator(List<ITrainingDueVarianceRule> Rules)
{
    public List<string> Validate(TrainingDueVariance item)
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

    public void RequireValid(TrainingDueVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TrainingDueVariance> items)
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

/// <summary>Functions of training due variance rules.</summary>
public static class TrainingDueVarianceRules
{
    public static TrainingDueVarianceValidator DefaultTrainingDueVarianceValidator()
    {
        return new TrainingDueVarianceValidator(
            new List<ITrainingDueVarianceRule>
            {
                new TrainingDueVarianceRequiredCodeRule("training due variance needs this field"),
                new TrainingDueVarianceNameLengthRule(120),
                new TrainingDueVarianceVersionRangeRule(80, 80),
                new TrainingDueVarianceRequiredGradeRule("training due variance needs an entry"),
                new TrainingDueVarianceThresholdRangeRule(5.4, 1315.5),
            });
    }
}
