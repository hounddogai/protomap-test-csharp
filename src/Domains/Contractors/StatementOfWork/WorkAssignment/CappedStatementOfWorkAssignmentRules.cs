namespace Acme.Workforce.Contractors.StatementOfWork.WorkAssignment;

/// <summary>A rule a capped statement of work assignment must pass. Check returns its problems, or none.</summary>
public interface ICappedStatementOfWorkAssignmentRule
{
    List<string> Check(CappedStatementOfWorkAssignment item);
}

public sealed record CappedStatementOfWorkAssignmentCostRangeRule(double Min, double Max) : ICappedStatementOfWorkAssignmentRule
{
    public List<string> Check(CappedStatementOfWorkAssignment item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CappedStatementOfWorkAssignmentLabelLengthRule(int MaxLength) : ICappedStatementOfWorkAssignmentRule
{
    public List<string> Check(CappedStatementOfWorkAssignment item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedStatementOfWorkAssignmentRequiredNameRule(string Message) : ICappedStatementOfWorkAssignmentRule
{
    public List<string> Check(CappedStatementOfWorkAssignment item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedStatementOfWorkAssignmentRequiredCodeRule(string Message) : ICappedStatementOfWorkAssignmentRule
{
    public List<string> Check(CappedStatementOfWorkAssignment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped statement of work assignment.</summary>
public sealed record CappedStatementOfWorkAssignmentValidator(List<ICappedStatementOfWorkAssignmentRule> Rules)
{
    public List<string> Validate(CappedStatementOfWorkAssignment item)
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

    public void RequireValid(CappedStatementOfWorkAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedStatementOfWorkAssignment> items)
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

/// <summary>Functions of capped statement of work assignment rules.</summary>
public static class CappedStatementOfWorkAssignmentRules
{
    public static CappedStatementOfWorkAssignmentValidator DefaultCappedStatementOfWorkAssignmentValidator()
    {
        return new CappedStatementOfWorkAssignmentValidator(
            new List<ICappedStatementOfWorkAssignmentRule>
            {
                new CappedStatementOfWorkAssignmentCostRangeRule(8.4, 1056.0),
                new CappedStatementOfWorkAssignmentLabelLengthRule(32),
                new CappedStatementOfWorkAssignmentRequiredNameRule(
                    "capped statement of work assignment needs this field"),
                new CappedStatementOfWorkAssignmentRequiredCodeRule(
                    "capped statement of work assignment needs this field"),
            });
    }
}
