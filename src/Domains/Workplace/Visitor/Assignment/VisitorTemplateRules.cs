namespace Acme.Workforce.Workplace.Visitor.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a visitor template must pass. Check returns its problems, or none.</summary>
public interface IVisitorTemplateRule
{
    List<string> Check(VisitorTemplate item);
}

public sealed record VisitorTemplateJobCodeLengthRule(int MaxLength) : IVisitorTemplateRule
{
    public List<string> Check(VisitorTemplate item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorTemplateRequiredNameRule(string Message) : IVisitorTemplateRule
{
    public List<string> Check(VisitorTemplate item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VisitorTemplateRequiredDepartmentRule(string Message) : IVisitorTemplateRule
{
    public List<string> Check(VisitorTemplate item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VisitorTemplateAllowedNameRule(List<string> Allowed) : IVisitorTemplateRule
{
    public List<string> Check(VisitorTemplate item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is out of range" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a visitor template.</summary>
public sealed record VisitorTemplateValidator(List<IVisitorTemplateRule> Rules)
{
    public List<string> Validate(VisitorTemplate item)
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

    public void RequireValid(VisitorTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VisitorTemplate> items)
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

/// <summary>Functions of visitor template rules.</summary>
public static class VisitorTemplateRules
{
    public static VisitorTemplateValidator DefaultVisitorTemplateValidator()
    {
        return new VisitorTemplateValidator(
            new List<IVisitorTemplateRule>
            {
                new VisitorTemplateJobCodeLengthRule(120),
                new VisitorTemplateRequiredNameRule("visitor template needs this field"),
                new VisitorTemplateRequiredDepartmentRule("visitor template needs a value"),
                new VisitorTemplateAllowedNameRule(new List<string>()),
            });
    }
}
