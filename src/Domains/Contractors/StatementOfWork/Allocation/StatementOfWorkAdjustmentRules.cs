namespace Acme.Workforce.Contractors.StatementOfWork.Allocation;

/// <summary>A rule a statement of work adjustment must pass. Check returns its problems, or none.</summary>
public interface IStatementOfWorkAdjustmentRule
{
    List<string> Check(StatementOfWorkAdjustment item);
}

public sealed record StatementOfWorkAdjustmentCreatedAtRangeRule(int Min, int Max) : IStatementOfWorkAdjustmentRule
{
    public List<string> Check(StatementOfWorkAdjustment item)
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

public sealed record StatementOfWorkAdjustmentAmountRangeRule(double Min, double Max) : IStatementOfWorkAdjustmentRule
{
    public List<string> Check(StatementOfWorkAdjustment item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkAdjustmentDepartmentLengthRule(int MaxLength) : IStatementOfWorkAdjustmentRule
{
    public List<string> Check(StatementOfWorkAdjustment item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a statement of work adjustment.</summary>
public sealed record StatementOfWorkAdjustmentValidator(List<IStatementOfWorkAdjustmentRule> Rules)
{
    public List<string> Validate(StatementOfWorkAdjustment item)
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

    public void RequireValid(StatementOfWorkAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StatementOfWorkAdjustment> items)
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

/// <summary>Functions of statement of work adjustment rules.</summary>
public static class StatementOfWorkAdjustmentRules
{
    public static StatementOfWorkAdjustmentValidator DefaultStatementOfWorkAdjustmentValidator()
    {
        return new StatementOfWorkAdjustmentValidator(
            new List<IStatementOfWorkAdjustmentRule>
            {
                new StatementOfWorkAdjustmentCreatedAtRangeRule(16, 32),
                new StatementOfWorkAdjustmentAmountRangeRule(10.0, 4968.6),
                new StatementOfWorkAdjustmentDepartmentLengthRule(86400),
            });
    }
}
