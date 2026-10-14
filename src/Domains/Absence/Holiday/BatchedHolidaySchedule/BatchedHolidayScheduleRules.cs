namespace Acme.Workforce.Absence.Holiday.BatchedHolidaySchedule;

/// <summary>A rule a batched holiday schedule must pass. Check returns its problems, or none.</summary>
public interface IBatchedHolidayScheduleRule
{
    List<string> Check(BatchedHolidaySchedule item);
}

public sealed record BatchedHolidayScheduleCodeLengthRule(int MaxLength) : IBatchedHolidayScheduleRule
{
    public List<string> Check(BatchedHolidaySchedule item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedHolidayScheduleRequiredCodeRule(string Message) : IBatchedHolidayScheduleRule
{
    public List<string> Check(BatchedHolidaySchedule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedHolidayScheduleRequiredJobCodeRule(string Message) : IBatchedHolidayScheduleRule
{
    public List<string> Check(BatchedHolidaySchedule item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedHolidayScheduleDescriptionLengthRule(int MaxLength) : IBatchedHolidayScheduleRule
{
    public List<string> Check(BatchedHolidaySchedule item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched holiday schedule.</summary>
public sealed record BatchedHolidayScheduleValidator(List<IBatchedHolidayScheduleRule> Rules)
{
    public List<string> Validate(BatchedHolidaySchedule item)
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

    public void RequireValid(BatchedHolidaySchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedHolidaySchedule> items)
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

/// <summary>Functions of batched holiday schedule rules.</summary>
public static class BatchedHolidayScheduleRules
{
    public static BatchedHolidayScheduleValidator DefaultBatchedHolidayScheduleValidator()
    {
        return new BatchedHolidayScheduleValidator(
            new List<IBatchedHolidayScheduleRule>
            {
                new BatchedHolidayScheduleCodeLengthRule(16),
                new BatchedHolidayScheduleRequiredCodeRule("batched holiday schedule needs an entry"),
                new BatchedHolidayScheduleRequiredJobCodeRule("batched holiday schedule needs a value"),
                new BatchedHolidayScheduleDescriptionLengthRule(1000),
            });
    }
}
