namespace Acme.Workforce.Contractors.StatementOfWork.Tier;

/// <summary>A rule a statement of work milestone must pass. Check returns its problems, or none.</summary>
public interface IStatementOfWorkMilestoneRule
{
    List<string> Check(StatementOfWorkMilestone item);
}

public sealed record StatementOfWorkMilestoneRequiredEmployeeIdRule(string Message) : IStatementOfWorkMilestoneRule
{
    public List<string> Check(StatementOfWorkMilestone item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkMilestoneWindowDaysRangeRule(int Min, int Max) : IStatementOfWorkMilestoneRule
{
    public List<string> Check(StatementOfWorkMilestone item)
    {
        var valueValue = item.WindowDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"window days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"window days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record StatementOfWorkMilestoneOwnerIdLengthRule(int MaxLength) : IStatementOfWorkMilestoneRule
{
    public List<string> Check(StatementOfWorkMilestone item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a statement of work milestone.</summary>
public sealed record StatementOfWorkMilestoneValidator(List<IStatementOfWorkMilestoneRule> Rules)
{
    public List<string> Validate(StatementOfWorkMilestone item)
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

    public void RequireValid(StatementOfWorkMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StatementOfWorkMilestone> items)
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

/// <summary>Functions of statement of work milestone rules.</summary>
public static class StatementOfWorkMilestoneRules
{
    public static StatementOfWorkMilestoneValidator DefaultStatementOfWorkMilestoneValidator()
    {
        return new StatementOfWorkMilestoneValidator(
            new List<IStatementOfWorkMilestoneRule>
            {
                new StatementOfWorkMilestoneRequiredEmployeeIdRule("statement of work milestone needs an entry"),
                new StatementOfWorkMilestoneWindowDaysRangeRule(120, 255),
                new StatementOfWorkMilestoneOwnerIdLengthRule(86400),
            });
    }
}
