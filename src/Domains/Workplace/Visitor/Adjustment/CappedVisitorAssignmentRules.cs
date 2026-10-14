namespace Acme.Workforce.Workplace.Visitor.Adjustment;

/// <summary>A rule a capped visitor assignment must pass. Check returns its problems, or none.</summary>
public interface ICappedVisitorAssignmentRule
{
    List<string> Check(CappedVisitorAssignment item);
}

public sealed record CappedVisitorAssignmentExpiresAtRangeRule(int Min, int Max) : ICappedVisitorAssignmentRule
{
    public List<string> Check(CappedVisitorAssignment item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CappedVisitorAssignmentRequiredSummaryRule(string Message) : ICappedVisitorAssignmentRule
{
    public List<string> Check(CappedVisitorAssignment item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedVisitorAssignmentTitleLengthRule(int MaxLength) : ICappedVisitorAssignmentRule
{
    public List<string> Check(CappedVisitorAssignment item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedVisitorAssignmentRequiredCodeRule(string Message) : ICappedVisitorAssignmentRule
{
    public List<string> Check(CappedVisitorAssignment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedVisitorAssignmentSummaryLengthRule(int MaxLength) : ICappedVisitorAssignmentRule
{
    public List<string> Check(CappedVisitorAssignment item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedVisitorAssignmentEmployeeIdLengthRule(int MaxLength) : ICappedVisitorAssignmentRule
{
    public List<string> Check(CappedVisitorAssignment item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped visitor assignment.</summary>
public sealed record CappedVisitorAssignmentValidator(List<ICappedVisitorAssignmentRule> Rules)
{
    public List<string> Validate(CappedVisitorAssignment item)
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

    public void RequireValid(CappedVisitorAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedVisitorAssignment> items)
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

/// <summary>Functions of capped visitor assignment rules.</summary>
public static class CappedVisitorAssignmentRules
{
    public static CappedVisitorAssignmentValidator DefaultCappedVisitorAssignmentValidator()
    {
        return new CappedVisitorAssignmentValidator(
            new List<ICappedVisitorAssignmentRule>
            {
                new CappedVisitorAssignmentExpiresAtRangeRule(64, 120),
                new CappedVisitorAssignmentRequiredSummaryRule("capped visitor assignment needs this field"),
                new CappedVisitorAssignmentTitleLengthRule(16),
                new CappedVisitorAssignmentRequiredCodeRule("capped visitor assignment needs this field"),
                new CappedVisitorAssignmentSummaryLengthRule(86400),
                new CappedVisitorAssignmentEmployeeIdLengthRule(3600),
            });
    }
}
