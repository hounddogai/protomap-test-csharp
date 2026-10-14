namespace Acme.Workforce.Compliance.TrainingDue.Summary;

/// <summary>A rule a training due summary must pass. Check returns its problems, or none.</summary>
public interface ITrainingDueSummaryRule
{
    List<string> Check(TrainingDueSummary item);
}

public sealed record TrainingDueSummaryJobCodeLengthRule(int MaxLength) : ITrainingDueSummaryRule
{
    public List<string> Check(TrainingDueSummary item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueSummaryFteRangeRule(double Min, double Max) : ITrainingDueSummaryRule
{
    public List<string> Check(TrainingDueSummary item)
    {
        var valueValue = item.Fte;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"fte {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"fte {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TrainingDueSummaryRequiredNameRule(string Message) : ITrainingDueSummaryRule
{
    public List<string> Check(TrainingDueSummary item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a training due summary.</summary>
public sealed record TrainingDueSummaryValidator(List<ITrainingDueSummaryRule> Rules)
{
    public List<string> Validate(TrainingDueSummary item)
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

    public void RequireValid(TrainingDueSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TrainingDueSummary> items)
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

/// <summary>Functions of training due summary rules.</summary>
public static class TrainingDueSummaryRules
{
    public static TrainingDueSummaryValidator DefaultTrainingDueSummaryValidator()
    {
        return new TrainingDueSummaryValidator(
            new List<ITrainingDueSummaryRule>
            {
                new TrainingDueSummaryJobCodeLengthRule(64),
                new TrainingDueSummaryFteRangeRule(6.2, 2571.1),
                new TrainingDueSummaryRequiredNameRule("training due summary needs an entry"),
            });
    }
}
