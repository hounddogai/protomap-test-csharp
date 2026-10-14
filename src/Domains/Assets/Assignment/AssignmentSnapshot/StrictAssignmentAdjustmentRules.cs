namespace Acme.Workforce.Assets.Assignment.AssignmentSnapshot;

/// <summary>A rule a strict assignment adjustment must pass. Check returns its problems, or none.</summary>
public interface IStrictAssignmentAdjustmentRule
{
    List<string> Check(StrictAssignmentAdjustment item);
}

public sealed record StrictAssignmentAdjustmentGradeLengthRule(int MaxLength) : IStrictAssignmentAdjustmentRule
{
    public List<string> Check(StrictAssignmentAdjustment item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictAssignmentAdjustmentFloorValueRangeRule(double Min, double Max) : IStrictAssignmentAdjustmentRule
{
    public List<string> Check(StrictAssignmentAdjustment item)
    {
        var valueValue = item.FloorValue;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"floor value {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"floor value {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record StrictAssignmentAdjustmentCodeLengthRule(int MaxLength) : IStrictAssignmentAdjustmentRule
{
    public List<string> Check(StrictAssignmentAdjustment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictAssignmentAdjustmentRequiredLabelRule(string Message) : IStrictAssignmentAdjustmentRule
{
    public List<string> Check(StrictAssignmentAdjustment item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictAssignmentAdjustmentNameLengthRule(int MaxLength) : IStrictAssignmentAdjustmentRule
{
    public List<string> Check(StrictAssignmentAdjustment item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictAssignmentAdjustmentRequiredGradeRule(string Message) : IStrictAssignmentAdjustmentRule
{
    public List<string> Check(StrictAssignmentAdjustment item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict assignment adjustment.</summary>
public sealed record StrictAssignmentAdjustmentValidator(List<IStrictAssignmentAdjustmentRule> Rules)
{
    public List<string> Validate(StrictAssignmentAdjustment item)
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

    public void RequireValid(StrictAssignmentAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictAssignmentAdjustment> items)
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

/// <summary>Functions of strict assignment adjustment rules.</summary>
public static class StrictAssignmentAdjustmentRules
{
    public static StrictAssignmentAdjustmentValidator DefaultStrictAssignmentAdjustmentValidator()
    {
        return new StrictAssignmentAdjustmentValidator(
            new List<IStrictAssignmentAdjustmentRule>
            {
                new StrictAssignmentAdjustmentGradeLengthRule(255),
                new StrictAssignmentAdjustmentFloorValueRangeRule(2.4, 518.4),
                new StrictAssignmentAdjustmentCodeLengthRule(3600),
                new StrictAssignmentAdjustmentRequiredLabelRule("strict assignment adjustment needs this field"),
                new StrictAssignmentAdjustmentNameLengthRule(255),
                new StrictAssignmentAdjustmentRequiredGradeRule("strict assignment adjustment needs this field"),
            });
    }
}
