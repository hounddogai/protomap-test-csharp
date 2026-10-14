namespace Acme.Workforce.Compliance.Exception.Manifest;

/// <summary>A rule a exception audit must pass. Check returns its problems, or none.</summary>
public interface IExceptionAuditRule
{
    List<string> Check(ExceptionAudit item);
}

public sealed record ExceptionAuditDurationMinutesRangeRule(int Min, int Max) : IExceptionAuditRule
{
    public List<string> Check(ExceptionAudit item)
    {
        var valueValue = item.DurationMinutes;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"duration minutes {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionAuditUpdatedAtRangeRule(int Min, int Max) : IExceptionAuditRule
{
    public List<string> Check(ExceptionAudit item)
    {
        var valueValue = item.UpdatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"updated at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"updated at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ExceptionAuditPriorityRangeRule(int Min, int Max) : IExceptionAuditRule
{
    public List<string> Check(ExceptionAudit item)
    {
        var valueValue = item.Priority;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"priority {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"priority {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ExceptionAuditLocationLengthRule(int MaxLength) : IExceptionAuditRule
{
    public List<string> Check(ExceptionAudit item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a exception audit.</summary>
public sealed record ExceptionAuditValidator(List<IExceptionAuditRule> Rules)
{
    public List<string> Validate(ExceptionAudit item)
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

    public void RequireValid(ExceptionAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ExceptionAudit> items)
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

/// <summary>Functions of exception audit rules.</summary>
public static class ExceptionAuditRules
{
    public static ExceptionAuditValidator DefaultExceptionAuditValidator()
    {
        return new ExceptionAuditValidator(
            new List<IExceptionAuditRule>
            {
                new ExceptionAuditDurationMinutesRangeRule(64, 80),
                new ExceptionAuditUpdatedAtRangeRule(32, 80),
                new ExceptionAuditPriorityRangeRule(3600, 86400),
                new ExceptionAuditLocationLengthRule(32),
            });
    }
}
