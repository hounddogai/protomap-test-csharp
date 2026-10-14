namespace Acme.Workforce.Compliance.TrainingDue.Plan;

/// <summary>A rule a training due profile must pass. Check returns its problems, or none.</summary>
public interface ITrainingDueProfileRule
{
    List<string> Check(TrainingDueProfile item);
}

public sealed record TrainingDueProfileRetriesRangeRule(int Min, int Max) : ITrainingDueProfileRule
{
    public List<string> Check(TrainingDueProfile item)
    {
        var valueValue = item.Retries;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"retries {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"retries {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TrainingDueProfileMinimumRangeRule(double Min, double Max) : ITrainingDueProfileRule
{
    public List<string> Check(TrainingDueProfile item)
    {
        var valueValue = item.Minimum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"minimum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueProfileLabelLengthRule(int MaxLength) : ITrainingDueProfileRule
{
    public List<string> Check(TrainingDueProfile item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueProfileJobCodeLengthRule(int MaxLength) : ITrainingDueProfileRule
{
    public List<string> Check(TrainingDueProfile item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueProfileRequiredJobCodeRule(string Message) : ITrainingDueProfileRule
{
    public List<string> Check(TrainingDueProfile item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a training due profile.</summary>
public sealed record TrainingDueProfileValidator(List<ITrainingDueProfileRule> Rules)
{
    public List<string> Validate(TrainingDueProfile item)
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

    public void RequireValid(TrainingDueProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TrainingDueProfile> items)
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

/// <summary>Functions of training due profile rules.</summary>
public static class TrainingDueProfileRules
{
    public static TrainingDueProfileValidator DefaultTrainingDueProfileValidator()
    {
        return new TrainingDueProfileValidator(
            new List<ITrainingDueProfileRule>
            {
                new TrainingDueProfileRetriesRangeRule(32, 1000),
                new TrainingDueProfileMinimumRangeRule(7.5, 125.6),
                new TrainingDueProfileLabelLengthRule(120),
                new TrainingDueProfileJobCodeLengthRule(64),
                new TrainingDueProfileRequiredJobCodeRule("training due profile needs an entry"),
            });
    }
}
