namespace Acme.Workforce.Compensation.BonusPool.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a default bonus pool cycle must pass. Check returns its problems, or none.</summary>
public interface IDefaultBonusPoolCycleRule
{
    List<string> Check(DefaultBonusPoolCycle item);
}

public sealed record DefaultBonusPoolCycleCodeLengthRule(int MaxLength) : IDefaultBonusPoolCycleRule
{
    public List<string> Check(DefaultBonusPoolCycle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultBonusPoolCycleRequiredCodeRule(string Message) : IDefaultBonusPoolCycleRule
{
    public List<string> Check(DefaultBonusPoolCycle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultBonusPoolCycleAllowedEmployeeIdRule(List<string> Allowed) : IDefaultBonusPoolCycleRule
{
    public List<string> Check(DefaultBonusPoolCycle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.EmployeeId)))
        {
            return new List<string> { $"employee id {item.EmployeeId} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record DefaultBonusPoolCycleEmployeeIdLengthRule(int MaxLength) : IDefaultBonusPoolCycleRule
{
    public List<string> Check(DefaultBonusPoolCycle item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default bonus pool cycle.</summary>
public sealed record DefaultBonusPoolCycleValidator(List<IDefaultBonusPoolCycleRule> Rules)
{
    public List<string> Validate(DefaultBonusPoolCycle item)
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

    public void RequireValid(DefaultBonusPoolCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultBonusPoolCycle> items)
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

/// <summary>Functions of default bonus pool cycle rules.</summary>
public static class DefaultBonusPoolCycleRules
{
    public static DefaultBonusPoolCycleValidator DefaultDefaultBonusPoolCycleValidator()
    {
        return new DefaultBonusPoolCycleValidator(
            new List<IDefaultBonusPoolCycleRule>
            {
                new DefaultBonusPoolCycleCodeLengthRule(32),
                new DefaultBonusPoolCycleRequiredCodeRule("default bonus pool cycle needs an entry"),
                new DefaultBonusPoolCycleAllowedEmployeeIdRule(new List<string>()),
                new DefaultBonusPoolCycleEmployeeIdLengthRule(32),
            });
    }
}
