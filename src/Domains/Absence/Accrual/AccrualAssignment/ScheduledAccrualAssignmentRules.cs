namespace Acme.Workforce.Absence.Accrual.AccrualAssignment;

/// <summary>A rule a scheduled accrual assignment must pass. Check returns its problems, or none.</summary>
public interface IScheduledAccrualAssignmentRule
{
    List<string> Check(ScheduledAccrualAssignment item);
}

public sealed record ScheduledAccrualAssignmentBudgetRangeRule(double Min, double Max) : IScheduledAccrualAssignmentRule
{
    public List<string> Check(ScheduledAccrualAssignment item)
    {
        var valueValue = item.Budget;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"budget {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"budget {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScheduledAccrualAssignmentRequiredLabelRule(string Message) : IScheduledAccrualAssignmentRule
{
    public List<string> Check(ScheduledAccrualAssignment item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledAccrualAssignmentCodeLengthRule(int MaxLength) : IScheduledAccrualAssignmentRule
{
    public List<string> Check(ScheduledAccrualAssignment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledAccrualAssignmentRequiredSummaryRule(string Message) : IScheduledAccrualAssignmentRule
{
    public List<string> Check(ScheduledAccrualAssignment item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled accrual assignment.</summary>
public sealed record ScheduledAccrualAssignmentValidator(List<IScheduledAccrualAssignmentRule> Rules)
{
    public List<string> Validate(ScheduledAccrualAssignment item)
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

    public void RequireValid(ScheduledAccrualAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledAccrualAssignment> items)
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

/// <summary>Functions of scheduled accrual assignment rules.</summary>
public static class ScheduledAccrualAssignmentRules
{
    public static ScheduledAccrualAssignmentValidator DefaultScheduledAccrualAssignmentValidator()
    {
        return new ScheduledAccrualAssignmentValidator(
            new List<IScheduledAccrualAssignmentRule>
            {
                new ScheduledAccrualAssignmentBudgetRangeRule(7.6, 4506.1),
                new ScheduledAccrualAssignmentRequiredLabelRule("scheduled accrual assignment needs this field"),
                new ScheduledAccrualAssignmentCodeLengthRule(86400),
                new ScheduledAccrualAssignmentRequiredSummaryRule("scheduled accrual assignment needs a value"),
            });
    }
}
