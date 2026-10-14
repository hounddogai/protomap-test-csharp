namespace Acme.Workforce.Assets.Assignment.Checkpoint;

/// <summary>A rule a assignment adjustment must pass. Check returns its problems, or none.</summary>
public interface IAssignmentAdjustmentRule
{
    List<string> Check(AssignmentAdjustment item);
}

public sealed record AssignmentAdjustmentRequiredNameRule(string Message) : IAssignmentAdjustmentRule
{
    public List<string> Check(AssignmentAdjustment item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssignmentAdjustmentCodeLengthRule(int MaxLength) : IAssignmentAdjustmentRule
{
    public List<string> Check(AssignmentAdjustment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentAdjustmentRequiredCodeRule(string Message) : IAssignmentAdjustmentRule
{
    public List<string> Check(AssignmentAdjustment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssignmentAdjustmentExpiresAtRangeRule(int Min, int Max) : IAssignmentAdjustmentRule
{
    public List<string> Check(AssignmentAdjustment item)
    {
        var valueValue = item.ExpiresAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"expires at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"expires at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AssignmentAdjustmentTitleLengthRule(int MaxLength) : IAssignmentAdjustmentRule
{
    public List<string> Check(AssignmentAdjustment item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentAdjustmentDurationMinutesRangeRule(int Min, int Max) : IAssignmentAdjustmentRule
{
    public List<string> Check(AssignmentAdjustment item)
    {
        var valueValue = item.DurationMinutes;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"duration minutes {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a assignment adjustment.</summary>
public sealed record AssignmentAdjustmentValidator(List<IAssignmentAdjustmentRule> Rules)
{
    public List<string> Validate(AssignmentAdjustment item)
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

    public void RequireValid(AssignmentAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssignmentAdjustment> items)
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

/// <summary>Functions of assignment adjustment rules.</summary>
public static class AssignmentAdjustmentRules
{
    public static AssignmentAdjustmentValidator DefaultAssignmentAdjustmentValidator()
    {
        return new AssignmentAdjustmentValidator(
            new List<IAssignmentAdjustmentRule>
            {
                new AssignmentAdjustmentRequiredNameRule("assignment adjustment needs this field"),
                new AssignmentAdjustmentCodeLengthRule(32),
                new AssignmentAdjustmentRequiredCodeRule("assignment adjustment needs a value"),
                new AssignmentAdjustmentExpiresAtRangeRule(32, 80),
                new AssignmentAdjustmentTitleLengthRule(255),
                new AssignmentAdjustmentDurationMinutesRangeRule(255, 1000),
            });
    }
}
