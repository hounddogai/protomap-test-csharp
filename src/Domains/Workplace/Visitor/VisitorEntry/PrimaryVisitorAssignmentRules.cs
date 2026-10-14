namespace Acme.Workforce.Workplace.Visitor.VisitorEntry;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a primary visitor assignment must pass. Check returns its problems, or none.</summary>
public interface IPrimaryVisitorAssignmentRule
{
    List<string> Check(PrimaryVisitorAssignment item);
}

public sealed record PrimaryVisitorAssignmentCodeLengthRule(int MaxLength) : IPrimaryVisitorAssignmentRule
{
    public List<string> Check(PrimaryVisitorAssignment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryVisitorAssignmentCodePrefixRule(string Prefix) : IPrimaryVisitorAssignmentRule
{
    public List<string> Check(PrimaryVisitorAssignment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryVisitorAssignmentCapacityRangeRule(int Min, int Max) : IPrimaryVisitorAssignmentRule
{
    public List<string> Check(PrimaryVisitorAssignment item)
    {
        var valueValue = item.Capacity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"capacity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryVisitorAssignmentAllowedNameRule(List<string> Allowed) : IPrimaryVisitorAssignmentRule
{
    public List<string> Check(PrimaryVisitorAssignment item)
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

public sealed record PrimaryVisitorAssignmentRequiredLabelRule(string Message) : IPrimaryVisitorAssignmentRule
{
    public List<string> Check(PrimaryVisitorAssignment item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary visitor assignment.</summary>
public sealed record PrimaryVisitorAssignmentValidator(List<IPrimaryVisitorAssignmentRule> Rules)
{
    public List<string> Validate(PrimaryVisitorAssignment item)
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

    public void RequireValid(PrimaryVisitorAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryVisitorAssignment> items)
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

/// <summary>Functions of primary visitor assignment rules.</summary>
public static class PrimaryVisitorAssignmentRules
{
    public static PrimaryVisitorAssignmentValidator DefaultPrimaryVisitorAssignmentValidator()
    {
        return new PrimaryVisitorAssignmentValidator(
            new List<IPrimaryVisitorAssignmentRule>
            {
                new PrimaryVisitorAssignmentCodeLengthRule(120),
                new PrimaryVisitorAssignmentCodePrefixRule("Q"),
                new PrimaryVisitorAssignmentCapacityRangeRule(120, 3600),
                new PrimaryVisitorAssignmentAllowedNameRule(new List<string>()),
                new PrimaryVisitorAssignmentRequiredLabelRule("primary visitor assignment needs an entry"),
            });
    }
}
