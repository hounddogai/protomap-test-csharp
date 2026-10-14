namespace Acme.Workforce.Assets.Assignment.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a assignment window must pass. Check returns its problems, or none.</summary>
public interface IAssignmentWindowRule
{
    List<string> Check(AssignmentWindow item);
}

public sealed record AssignmentWindowRequiredNameRule(string Message) : IAssignmentWindowRule
{
    public List<string> Check(AssignmentWindow item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssignmentWindowRequiredCodeRule(string Message) : IAssignmentWindowRule
{
    public List<string> Check(AssignmentWindow item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssignmentWindowDepartmentLengthRule(int MaxLength) : IAssignmentWindowRule
{
    public List<string> Check(AssignmentWindow item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentWindowAllowedCodeRule(List<string> Allowed) : IAssignmentWindowRule
{
    public List<string> Check(AssignmentWindow item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentWindowRequiredEmployeeIdRule(string Message) : IAssignmentWindowRule
{
    public List<string> Check(AssignmentWindow item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a assignment window.</summary>
public sealed record AssignmentWindowValidator(List<IAssignmentWindowRule> Rules)
{
    public List<string> Validate(AssignmentWindow item)
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

    public void RequireValid(AssignmentWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssignmentWindow> items)
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

/// <summary>Functions of assignment window rules.</summary>
public static class AssignmentWindowRules
{
    public static AssignmentWindowValidator DefaultAssignmentWindowValidator()
    {
        return new AssignmentWindowValidator(
            new List<IAssignmentWindowRule>
            {
                new AssignmentWindowRequiredNameRule("assignment window needs a value"),
                new AssignmentWindowRequiredCodeRule("assignment window needs a value"),
                new AssignmentWindowDepartmentLengthRule(16),
                new AssignmentWindowAllowedCodeRule(new List<string>()),
                new AssignmentWindowRequiredEmployeeIdRule("assignment window needs an entry"),
            });
    }
}
