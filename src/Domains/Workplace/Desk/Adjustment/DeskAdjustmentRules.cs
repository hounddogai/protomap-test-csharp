namespace Acme.Workforce.Workplace.Desk.Adjustment;

/// <summary>A rule a desk adjustment must pass. Check returns its problems, or none.</summary>
public interface IDeskAdjustmentRule
{
    List<string> Check(DeskAdjustment item);
}

public sealed record DeskAdjustmentTitleLengthRule(int MaxLength) : IDeskAdjustmentRule
{
    public List<string> Check(DeskAdjustment item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeskAdjustmentRequiredDepartmentRule(string Message) : IDeskAdjustmentRule
{
    public List<string> Check(DeskAdjustment item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeskAdjustmentCodeLengthRule(int MaxLength) : IDeskAdjustmentRule
{
    public List<string> Check(DeskAdjustment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeskAdjustmentRequiredTitleRule(string Message) : IDeskAdjustmentRule
{
    public List<string> Check(DeskAdjustment item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a desk adjustment.</summary>
public sealed record DeskAdjustmentValidator(List<IDeskAdjustmentRule> Rules)
{
    public List<string> Validate(DeskAdjustment item)
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

    public void RequireValid(DeskAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeskAdjustment> items)
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

/// <summary>Functions of desk adjustment rules.</summary>
public static class DeskAdjustmentRules
{
    public static DeskAdjustmentValidator DefaultDeskAdjustmentValidator()
    {
        return new DeskAdjustmentValidator(
            new List<IDeskAdjustmentRule>
            {
                new DeskAdjustmentTitleLengthRule(1000),
                new DeskAdjustmentRequiredDepartmentRule("desk adjustment needs a value"),
                new DeskAdjustmentCodeLengthRule(1000),
                new DeskAdjustmentRequiredTitleRule("desk adjustment needs a value"),
            });
    }
}
