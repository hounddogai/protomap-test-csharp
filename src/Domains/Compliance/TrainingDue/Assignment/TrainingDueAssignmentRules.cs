namespace Acme.Workforce.Compliance.TrainingDue.Assignment;

/// <summary>A rule a training due assignment must pass. Check returns its problems, or none.</summary>
public interface ITrainingDueAssignmentRule
{
    List<string> Check(TrainingDueAssignment item);
}

public sealed record TrainingDueAssignmentMarginRangeRule(double Min, double Max) : ITrainingDueAssignmentRule
{
    public List<string> Check(TrainingDueAssignment item)
    {
        var valueValue = item.Margin;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"margin {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"margin {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TrainingDueAssignmentCostCenterLengthRule(int MaxLength) : ITrainingDueAssignmentRule
{
    public List<string> Check(TrainingDueAssignment item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueAssignmentApprovedGuardRule(double Limit) : ITrainingDueAssignmentRule
{
    public List<string> Check(TrainingDueAssignment item)
    {
        if (item.Approved && item.Margin > Limit)
        {
            return new List<string> { $"margin {item.Margin} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueAssignmentRequiredDepartmentRule(string Message) : ITrainingDueAssignmentRule
{
    public List<string> Check(TrainingDueAssignment item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a training due assignment.</summary>
public sealed record TrainingDueAssignmentValidator(List<ITrainingDueAssignmentRule> Rules)
{
    public List<string> Validate(TrainingDueAssignment item)
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

    public void RequireValid(TrainingDueAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TrainingDueAssignment> items)
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

/// <summary>Functions of training due assignment rules.</summary>
public static class TrainingDueAssignmentRules
{
    public static TrainingDueAssignmentValidator DefaultTrainingDueAssignmentValidator()
    {
        return new TrainingDueAssignmentValidator(
            new List<ITrainingDueAssignmentRule>
            {
                new TrainingDueAssignmentMarginRangeRule(1.4, 2422.3),
                new TrainingDueAssignmentCostCenterLengthRule(120),
                new TrainingDueAssignmentApprovedGuardRule(1.5),
                new TrainingDueAssignmentRequiredDepartmentRule("training due assignment needs this field"),
            });
    }
}
