namespace Acme.Workforce.Workplace.Visitor.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a visitor mapping must pass. Check returns its problems, or none.</summary>
public interface IVisitorMappingRule
{
    List<string> Check(VisitorMapping item);
}

public sealed record VisitorMappingAllowedNameRule(List<string> Allowed) : IVisitorMappingRule
{
    public List<string> Check(VisitorMapping item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record VisitorMappingRequiredTimezoneRule(string Message) : IVisitorMappingRule
{
    public List<string> Check(VisitorMapping item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VisitorMappingRequiredNameRule(string Message) : IVisitorMappingRule
{
    public List<string> Check(VisitorMapping item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a visitor mapping.</summary>
public sealed record VisitorMappingValidator(List<IVisitorMappingRule> Rules)
{
    public List<string> Validate(VisitorMapping item)
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

    public void RequireValid(VisitorMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VisitorMapping> items)
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

/// <summary>Functions of visitor mapping rules.</summary>
public static class VisitorMappingRules
{
    public static VisitorMappingValidator DefaultVisitorMappingValidator()
    {
        return new VisitorMappingValidator(
            new List<IVisitorMappingRule>
            {
                new VisitorMappingAllowedNameRule(new List<string> { "manual" }),
                new VisitorMappingRequiredTimezoneRule("visitor mapping needs an entry"),
                new VisitorMappingRequiredNameRule("visitor mapping needs an entry"),
            });
    }
}
