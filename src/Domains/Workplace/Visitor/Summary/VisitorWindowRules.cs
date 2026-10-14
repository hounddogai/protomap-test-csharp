namespace Acme.Workforce.Workplace.Visitor.Summary;

/// <summary>A rule a visitor window must pass. Check returns its problems, or none.</summary>
public interface IVisitorWindowRule
{
    List<string> Check(VisitorWindow item);
}

public sealed record VisitorWindowRequiredEmployeeIdRule(string Message) : IVisitorWindowRule
{
    public List<string> Check(VisitorWindow item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VisitorWindowCodeLengthRule(int MaxLength) : IVisitorWindowRule
{
    public List<string> Check(VisitorWindow item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorWindowNameLengthRule(int MaxLength) : IVisitorWindowRule
{
    public List<string> Check(VisitorWindow item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a visitor window.</summary>
public sealed record VisitorWindowValidator(List<IVisitorWindowRule> Rules)
{
    public List<string> Validate(VisitorWindow item)
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

    public void RequireValid(VisitorWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VisitorWindow> items)
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

/// <summary>Functions of visitor window rules.</summary>
public static class VisitorWindowRules
{
    public static VisitorWindowValidator DefaultVisitorWindowValidator()
    {
        return new VisitorWindowValidator(
            new List<IVisitorWindowRule>
            {
                new VisitorWindowRequiredEmployeeIdRule("visitor window needs this field"),
                new VisitorWindowCodeLengthRule(3600),
                new VisitorWindowNameLengthRule(255),
            });
    }
}
