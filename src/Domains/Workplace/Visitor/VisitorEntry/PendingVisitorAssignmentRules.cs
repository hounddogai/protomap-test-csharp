namespace Acme.Workforce.Workplace.Visitor.VisitorEntry;

/// <summary>A rule a pending visitor assignment must pass. Check returns its problems, or none.</summary>
public interface IPendingVisitorAssignmentRule
{
    List<string> Check(PendingVisitorAssignment item);
}

public sealed record PendingVisitorAssignmentCodeLengthRule(int MaxLength) : IPendingVisitorAssignmentRule
{
    public List<string> Check(PendingVisitorAssignment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingVisitorAssignmentNameLengthRule(int MaxLength) : IPendingVisitorAssignmentRule
{
    public List<string> Check(PendingVisitorAssignment item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingVisitorAssignmentLabelLengthRule(int MaxLength) : IPendingVisitorAssignmentRule
{
    public List<string> Check(PendingVisitorAssignment item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingVisitorAssignmentRequiredLabelRule(string Message) : IPendingVisitorAssignmentRule
{
    public List<string> Check(PendingVisitorAssignment item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingVisitorAssignmentRequiredCodeRule(string Message) : IPendingVisitorAssignmentRule
{
    public List<string> Check(PendingVisitorAssignment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending visitor assignment.</summary>
public sealed record PendingVisitorAssignmentValidator(List<IPendingVisitorAssignmentRule> Rules)
{
    public List<string> Validate(PendingVisitorAssignment item)
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

    public void RequireValid(PendingVisitorAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingVisitorAssignment> items)
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

/// <summary>Functions of pending visitor assignment rules.</summary>
public static class PendingVisitorAssignmentRules
{
    public static PendingVisitorAssignmentValidator DefaultPendingVisitorAssignmentValidator()
    {
        return new PendingVisitorAssignmentValidator(
            new List<IPendingVisitorAssignmentRule>
            {
                new PendingVisitorAssignmentCodeLengthRule(32),
                new PendingVisitorAssignmentNameLengthRule(120),
                new PendingVisitorAssignmentLabelLengthRule(32),
                new PendingVisitorAssignmentRequiredLabelRule("pending visitor assignment needs this field"),
                new PendingVisitorAssignmentRequiredCodeRule("pending visitor assignment needs an entry"),
            });
    }
}
