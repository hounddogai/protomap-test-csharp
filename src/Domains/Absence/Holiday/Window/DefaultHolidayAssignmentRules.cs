namespace Acme.Workforce.Absence.Holiday.Window;

/// <summary>A rule a default holiday assignment must pass. Check returns its problems, or none.</summary>
public interface IDefaultHolidayAssignmentRule
{
    List<string> Check(DefaultHolidayAssignment item);
}

public sealed record DefaultHolidayAssignmentRequiredCodeRule(string Message) : IDefaultHolidayAssignmentRule
{
    public List<string> Check(DefaultHolidayAssignment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultHolidayAssignmentBudgetRangeRule(double Min, double Max) : IDefaultHolidayAssignmentRule
{
    public List<string> Check(DefaultHolidayAssignment item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultHolidayAssignmentRequiredTitleRule(string Message) : IDefaultHolidayAssignmentRule
{
    public List<string> Check(DefaultHolidayAssignment item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default holiday assignment.</summary>
public sealed record DefaultHolidayAssignmentValidator(List<IDefaultHolidayAssignmentRule> Rules)
{
    public List<string> Validate(DefaultHolidayAssignment item)
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

    public void RequireValid(DefaultHolidayAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultHolidayAssignment> items)
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

/// <summary>Functions of default holiday assignment rules.</summary>
public static class DefaultHolidayAssignmentRules
{
    public static DefaultHolidayAssignmentValidator DefaultDefaultHolidayAssignmentValidator()
    {
        return new DefaultHolidayAssignmentValidator(
            new List<IDefaultHolidayAssignmentRule>
            {
                new DefaultHolidayAssignmentRequiredCodeRule("default holiday assignment needs a value"),
                new DefaultHolidayAssignmentBudgetRangeRule(1.2, 1488.8),
                new DefaultHolidayAssignmentRequiredTitleRule("default holiday assignment needs a value"),
            });
    }
}
