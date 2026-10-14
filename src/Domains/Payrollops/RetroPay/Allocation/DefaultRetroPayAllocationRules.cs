namespace Acme.Workforce.Payrollops.RetroPay.Allocation;

/// <summary>A rule a default retro pay allocation must pass. Check returns its problems, or none.</summary>
public interface IDefaultRetroPayAllocationRule
{
    List<string> Check(DefaultRetroPayAllocation item);
}

public sealed record DefaultRetroPayAllocationRequiredDepartmentRule(string Message) : IDefaultRetroPayAllocationRule
{
    public List<string> Check(DefaultRetroPayAllocation item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultRetroPayAllocationDepartmentLengthRule(int MaxLength) : IDefaultRetroPayAllocationRule
{
    public List<string> Check(DefaultRetroPayAllocation item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultRetroPayAllocationRequiredGradeRule(string Message) : IDefaultRetroPayAllocationRule
{
    public List<string> Check(DefaultRetroPayAllocation item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultRetroPayAllocationTitleLengthRule(int MaxLength) : IDefaultRetroPayAllocationRule
{
    public List<string> Check(DefaultRetroPayAllocation item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultRetroPayAllocationGradeLengthRule(int MaxLength) : IDefaultRetroPayAllocationRule
{
    public List<string> Check(DefaultRetroPayAllocation item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default retro pay allocation.</summary>
public sealed record DefaultRetroPayAllocationValidator(List<IDefaultRetroPayAllocationRule> Rules)
{
    public List<string> Validate(DefaultRetroPayAllocation item)
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

    public void RequireValid(DefaultRetroPayAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultRetroPayAllocation> items)
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

/// <summary>Functions of default retro pay allocation rules.</summary>
public static class DefaultRetroPayAllocationRules
{
    public static DefaultRetroPayAllocationValidator DefaultDefaultRetroPayAllocationValidator()
    {
        return new DefaultRetroPayAllocationValidator(
            new List<IDefaultRetroPayAllocationRule>
            {
                new DefaultRetroPayAllocationRequiredDepartmentRule("default retro pay allocation needs this field"),
                new DefaultRetroPayAllocationDepartmentLengthRule(3600),
                new DefaultRetroPayAllocationRequiredGradeRule("default retro pay allocation needs a value"),
                new DefaultRetroPayAllocationTitleLengthRule(80),
                new DefaultRetroPayAllocationGradeLengthRule(1000),
            });
    }
}
