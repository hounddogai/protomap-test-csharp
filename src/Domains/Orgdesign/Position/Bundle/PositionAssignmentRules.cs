namespace Acme.Workforce.Orgdesign.Position.Bundle;

/// <summary>A rule a position assignment must pass. Check returns its problems, or none.</summary>
public interface IPositionAssignmentRule
{
    List<string> Check(PositionAssignment item);
}

public sealed record PositionAssignmentVisibleGuardRule(double Limit) : IPositionAssignmentRule
{
    public List<string> Check(PositionAssignment item)
    {
        if (item.Visible && item.Weight > Limit)
        {
            return new List<string> { $"weight {item.Weight} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record PositionAssignmentEmployeeIdLengthRule(int MaxLength) : IPositionAssignmentRule
{
    public List<string> Check(PositionAssignment item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PositionAssignmentCapacityRangeRule(int Min, int Max) : IPositionAssignmentRule
{
    public List<string> Check(PositionAssignment item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a position assignment.</summary>
public sealed record PositionAssignmentValidator(List<IPositionAssignmentRule> Rules)
{
    public List<string> Validate(PositionAssignment item)
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

    public void RequireValid(PositionAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PositionAssignment> items)
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

/// <summary>Functions of position assignment rules.</summary>
public static class PositionAssignmentRules
{
    public static PositionAssignmentValidator DefaultPositionAssignmentValidator()
    {
        return new PositionAssignmentValidator(
            new List<IPositionAssignmentRule>
            {
                new PositionAssignmentVisibleGuardRule(3.1),
                new PositionAssignmentEmployeeIdLengthRule(255),
                new PositionAssignmentCapacityRangeRule(80, 255),
            });
    }
}
