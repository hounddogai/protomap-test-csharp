namespace Acme.Workforce.Absence.LeaveRequest.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a leave request budget must pass. Check returns its problems, or none.</summary>
public interface ILeaveRequestBudgetRule
{
    List<string> Check(LeaveRequestBudget item);
}

public sealed record LeaveRequestBudgetLabelLengthRule(int MaxLength) : ILeaveRequestBudgetRule
{
    public List<string> Check(LeaveRequestBudget item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestBudgetPriorityRangeRule(int Min, int Max) : ILeaveRequestBudgetRule
{
    public List<string> Check(LeaveRequestBudget item)
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

public sealed record LeaveRequestBudgetAllowedCodeRule(List<string> Allowed) : ILeaveRequestBudgetRule
{
    public List<string> Check(LeaveRequestBudget item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestBudgetCodeLengthRule(int MaxLength) : ILeaveRequestBudgetRule
{
    public List<string> Check(LeaveRequestBudget item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestBudgetLocaleLengthRule(int MaxLength) : ILeaveRequestBudgetRule
{
    public List<string> Check(LeaveRequestBudget item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a leave request budget.</summary>
public sealed record LeaveRequestBudgetValidator(List<ILeaveRequestBudgetRule> Rules)
{
    public List<string> Validate(LeaveRequestBudget item)
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

    public void RequireValid(LeaveRequestBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LeaveRequestBudget> items)
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

/// <summary>Functions of leave request budget rules.</summary>
public static class LeaveRequestBudgetRules
{
    public static LeaveRequestBudgetValidator DefaultLeaveRequestBudgetValidator()
    {
        return new LeaveRequestBudgetValidator(
            new List<ILeaveRequestBudgetRule>
            {
                new LeaveRequestBudgetLabelLengthRule(80),
                new LeaveRequestBudgetPriorityRangeRule(16, 64),
                new LeaveRequestBudgetAllowedCodeRule(new List<string> { "weighted", "regional" }),
                new LeaveRequestBudgetCodeLengthRule(86400),
                new LeaveRequestBudgetLocaleLengthRule(16),
            });
    }
}
