namespace Acme.Workforce.Contractors.StatementOfWork.Assignment;

/// <summary>A rule a statement of work assignment must pass. Check returns its problems, or none.</summary>
public interface IStatementOfWorkAssignmentRule
{
    List<string> Check(StatementOfWorkAssignment item);
}

public sealed record StatementOfWorkAssignmentCodeLengthRule(int MaxLength) : IStatementOfWorkAssignmentRule
{
    public List<string> Check(StatementOfWorkAssignment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkAssignmentFloorValueRangeRule(double Min, double Max) : IStatementOfWorkAssignmentRule
{
    public List<string> Check(StatementOfWorkAssignment item)
    {
        var valueValue = item.FloorValue;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"floor value {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkAssignmentCostCenterLengthRule(int MaxLength) : IStatementOfWorkAssignmentRule
{
    public List<string> Check(StatementOfWorkAssignment item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkAssignmentRequiredNameRule(string Message) : IStatementOfWorkAssignmentRule
{
    public List<string> Check(StatementOfWorkAssignment item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a statement of work assignment.</summary>
public sealed record StatementOfWorkAssignmentValidator(List<IStatementOfWorkAssignmentRule> Rules)
{
    public List<string> Validate(StatementOfWorkAssignment item)
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

    public void RequireValid(StatementOfWorkAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StatementOfWorkAssignment> items)
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

/// <summary>Functions of statement of work assignment rules.</summary>
public static class StatementOfWorkAssignmentRules
{
    public static StatementOfWorkAssignmentValidator DefaultStatementOfWorkAssignmentValidator()
    {
        return new StatementOfWorkAssignmentValidator(
            new List<IStatementOfWorkAssignmentRule>
            {
                new StatementOfWorkAssignmentCodeLengthRule(1000),
                new StatementOfWorkAssignmentFloorValueRangeRule(4.3, 2046.5),
                new StatementOfWorkAssignmentCostCenterLengthRule(3600),
                new StatementOfWorkAssignmentRequiredNameRule("statement of work assignment needs this field"),
            });
    }
}
