namespace Acme.Workforce.Absence.Policy.Assignment;

/// <summary>A rule a default policy assignment must pass. Check returns its problems, or none.</summary>
public interface IDefaultPolicyAssignmentRule
{
    List<string> Check(DefaultPolicyAssignment item);
}

public sealed record DefaultPolicyAssignmentNameLengthRule(int MaxLength) : IDefaultPolicyAssignmentRule
{
    public List<string> Check(DefaultPolicyAssignment item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultPolicyAssignmentCostRangeRule(double Min, double Max) : IDefaultPolicyAssignmentRule
{
    public List<string> Check(DefaultPolicyAssignment item)
    {
        var valueValue = item.Cost;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"cost {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"cost {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DefaultPolicyAssignmentCodeLengthRule(int MaxLength) : IDefaultPolicyAssignmentRule
{
    public List<string> Check(DefaultPolicyAssignment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultPolicyAssignmentRequiredTitleRule(string Message) : IDefaultPolicyAssignmentRule
{
    public List<string> Check(DefaultPolicyAssignment item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultPolicyAssignmentLeaveDaysRangeRule(int Min, int Max) : IDefaultPolicyAssignmentRule
{
    public List<string> Check(DefaultPolicyAssignment item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default policy assignment.</summary>
public sealed record DefaultPolicyAssignmentValidator(List<IDefaultPolicyAssignmentRule> Rules)
{
    public List<string> Validate(DefaultPolicyAssignment item)
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

    public void RequireValid(DefaultPolicyAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultPolicyAssignment> items)
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

/// <summary>Functions of default policy assignment rules.</summary>
public static class DefaultPolicyAssignmentRules
{
    public static DefaultPolicyAssignmentValidator DefaultDefaultPolicyAssignmentValidator()
    {
        return new DefaultPolicyAssignmentValidator(
            new List<IDefaultPolicyAssignmentRule>
            {
                new DefaultPolicyAssignmentNameLengthRule(16),
                new DefaultPolicyAssignmentCostRangeRule(3.6, 1994.6),
                new DefaultPolicyAssignmentCodeLengthRule(255),
                new DefaultPolicyAssignmentRequiredTitleRule("default policy assignment needs a value"),
                new DefaultPolicyAssignmentLeaveDaysRangeRule(32, 1000),
            });
    }
}
