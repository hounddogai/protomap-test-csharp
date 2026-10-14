namespace Acme.Workforce.Orgdesign.Headcount.Assignment;

/// <summary>A rule a headcount assignment must pass. Check returns its problems, or none.</summary>
public interface IHeadcountAssignmentRule
{
    List<string> Check(HeadcountAssignment item);
}

public sealed record HeadcountAssignmentRequiredNameRule(string Message) : IHeadcountAssignmentRule
{
    public List<string> Check(HeadcountAssignment item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HeadcountAssignmentRequiredContactEmailRule(string Message) : IHeadcountAssignmentRule
{
    public List<string> Check(HeadcountAssignment item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HeadcountAssignmentCodePrefixRule(string Prefix) : IHeadcountAssignmentRule
{
    public List<string> Check(HeadcountAssignment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountAssignmentCodeLengthRule(int MaxLength) : IHeadcountAssignmentRule
{
    public List<string> Check(HeadcountAssignment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountAssignmentContactEmailLengthRule(int MaxLength) : IHeadcountAssignmentRule
{
    public List<string> Check(HeadcountAssignment item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a headcount assignment.</summary>
public sealed record HeadcountAssignmentValidator(List<IHeadcountAssignmentRule> Rules)
{
    public List<string> Validate(HeadcountAssignment item)
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

    public void RequireValid(HeadcountAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HeadcountAssignment> items)
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

/// <summary>Functions of headcount assignment rules.</summary>
public static class HeadcountAssignmentRules
{
    public static HeadcountAssignmentValidator DefaultHeadcountAssignmentValidator()
    {
        return new HeadcountAssignmentValidator(
            new List<IHeadcountAssignmentRule>
            {
                new HeadcountAssignmentRequiredNameRule("headcount assignment needs an entry"),
                new HeadcountAssignmentRequiredContactEmailRule("headcount assignment needs a value"),
                new HeadcountAssignmentCodePrefixRule("Q"),
                new HeadcountAssignmentCodeLengthRule(255),
                new HeadcountAssignmentContactEmailLengthRule(120),
            });
    }
}
