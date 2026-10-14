namespace Acme.Workforce.Contractors.StatementOfWork.Assignment;

/// <summary>A rule a statement of work schedule must pass. Check returns its problems, or none.</summary>
public interface IStatementOfWorkScheduleRule
{
    List<string> Check(StatementOfWorkSchedule item);
}

public sealed record StatementOfWorkScheduleRequiredNameRule(string Message) : IStatementOfWorkScheduleRule
{
    public List<string> Check(StatementOfWorkSchedule item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkScheduleRequiredCodeRule(string Message) : IStatementOfWorkScheduleRule
{
    public List<string> Check(StatementOfWorkSchedule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkSchedulePinnedGuardRule(double Limit) : IStatementOfWorkScheduleRule
{
    public List<string> Check(StatementOfWorkSchedule item)
    {
        if (item.Pinned && item.Minimum > Limit)
        {
            return new List<string> { $"minimum {item.Minimum} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkScheduleCreatedAtRangeRule(int Min, int Max) : IStatementOfWorkScheduleRule
{
    public List<string> Check(StatementOfWorkSchedule item)
    {
        var valueValue = item.CreatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"created at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"created at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record StatementOfWorkScheduleNameLengthRule(int MaxLength) : IStatementOfWorkScheduleRule
{
    public List<string> Check(StatementOfWorkSchedule item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a statement of work schedule.</summary>
public sealed record StatementOfWorkScheduleValidator(List<IStatementOfWorkScheduleRule> Rules)
{
    public List<string> Validate(StatementOfWorkSchedule item)
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

    public void RequireValid(StatementOfWorkSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StatementOfWorkSchedule> items)
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

/// <summary>Functions of statement of work schedule rules.</summary>
public static class StatementOfWorkScheduleRules
{
    public static StatementOfWorkScheduleValidator DefaultStatementOfWorkScheduleValidator()
    {
        return new StatementOfWorkScheduleValidator(
            new List<IStatementOfWorkScheduleRule>
            {
                new StatementOfWorkScheduleRequiredNameRule("statement of work schedule needs a value"),
                new StatementOfWorkScheduleRequiredCodeRule("statement of work schedule needs a value"),
                new StatementOfWorkSchedulePinnedGuardRule(9.5),
                new StatementOfWorkScheduleCreatedAtRangeRule(80, 120),
                new StatementOfWorkScheduleNameLengthRule(86400),
            });
    }
}
