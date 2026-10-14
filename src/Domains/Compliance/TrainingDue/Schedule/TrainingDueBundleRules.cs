namespace Acme.Workforce.Compliance.TrainingDue.Schedule;

/// <summary>A rule a training due bundle must pass. Check returns its problems, or none.</summary>
public interface ITrainingDueBundleRule
{
    List<string> Check(TrainingDueBundle item);
}

public sealed record TrainingDueBundleCodeLengthRule(int MaxLength) : ITrainingDueBundleRule
{
    public List<string> Check(TrainingDueBundle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueBundleRequiredTimezoneRule(string Message) : ITrainingDueBundleRule
{
    public List<string> Check(TrainingDueBundle item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueBundleRequiredTitleRule(string Message) : ITrainingDueBundleRule
{
    public List<string> Check(TrainingDueBundle item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueBundleRequiredCodeRule(string Message) : ITrainingDueBundleRule
{
    public List<string> Check(TrainingDueBundle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a training due bundle.</summary>
public sealed record TrainingDueBundleValidator(List<ITrainingDueBundleRule> Rules)
{
    public List<string> Validate(TrainingDueBundle item)
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

    public void RequireValid(TrainingDueBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TrainingDueBundle> items)
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

/// <summary>Functions of training due bundle rules.</summary>
public static class TrainingDueBundleRules
{
    public static TrainingDueBundleValidator DefaultTrainingDueBundleValidator()
    {
        return new TrainingDueBundleValidator(
            new List<ITrainingDueBundleRule>
            {
                new TrainingDueBundleCodeLengthRule(80),
                new TrainingDueBundleRequiredTimezoneRule("training due bundle needs an entry"),
                new TrainingDueBundleRequiredTitleRule("training due bundle needs a value"),
                new TrainingDueBundleRequiredCodeRule("training due bundle needs this field"),
            });
    }
}
