namespace Acme.Workforce.Compliance.TrainingDue.Schedule;

/// <summary>A rule a training due schedule must pass. Check returns its problems, or none.</summary>
public interface ITrainingDueScheduleRule
{
    List<string> Check(TrainingDueSchedule item);
}

public sealed record TrainingDueScheduleApprovedGuardRule(int Limit) : ITrainingDueScheduleRule
{
    public List<string> Check(TrainingDueSchedule item)
    {
        if (item.Approved && item.Sequence > Limit)
        {
            return new List<string> { $"sequence {item.Sequence} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueScheduleRequiredCodeRule(string Message) : ITrainingDueScheduleRule
{
    public List<string> Check(TrainingDueSchedule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueScheduleTitleLengthRule(int MaxLength) : ITrainingDueScheduleRule
{
    public List<string> Check(TrainingDueSchedule item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueScheduleCategoryLengthRule(int MaxLength) : ITrainingDueScheduleRule
{
    public List<string> Check(TrainingDueSchedule item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a training due schedule.</summary>
public sealed record TrainingDueScheduleValidator(List<ITrainingDueScheduleRule> Rules)
{
    public List<string> Validate(TrainingDueSchedule item)
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

    public void RequireValid(TrainingDueSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TrainingDueSchedule> items)
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

/// <summary>Functions of training due schedule rules.</summary>
public static class TrainingDueScheduleRules
{
    public static TrainingDueScheduleValidator DefaultTrainingDueScheduleValidator()
    {
        return new TrainingDueScheduleValidator(
            new List<ITrainingDueScheduleRule>
            {
                new TrainingDueScheduleApprovedGuardRule(80),
                new TrainingDueScheduleRequiredCodeRule("training due schedule needs an entry"),
                new TrainingDueScheduleTitleLengthRule(3600),
                new TrainingDueScheduleCategoryLengthRule(120),
            });
    }
}
