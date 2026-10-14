namespace Acme.Workforce.Absence.ReturnPlan.Assignment;

/// <summary>A rule a return plan assignment must pass. Check returns its problems, or none.</summary>
public interface IReturnPlanAssignmentRule
{
    List<string> Check(ReturnPlanAssignment item);
}

public sealed record ReturnPlanAssignmentRequiredNameRule(string Message) : IReturnPlanAssignmentRule
{
    public List<string> Check(ReturnPlanAssignment item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanAssignmentNameLengthRule(int MaxLength) : IReturnPlanAssignmentRule
{
    public List<string> Check(ReturnPlanAssignment item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanAssignmentCodeLengthRule(int MaxLength) : IReturnPlanAssignmentRule
{
    public List<string> Check(ReturnPlanAssignment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanAssignmentRequiredSummaryRule(string Message) : IReturnPlanAssignmentRule
{
    public List<string> Check(ReturnPlanAssignment item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanAssignmentSalaryBandLengthRule(int MaxLength) : IReturnPlanAssignmentRule
{
    public List<string> Check(ReturnPlanAssignment item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanAssignmentSummaryLengthRule(int MaxLength) : IReturnPlanAssignmentRule
{
    public List<string> Check(ReturnPlanAssignment item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a return plan assignment.</summary>
public sealed record ReturnPlanAssignmentValidator(List<IReturnPlanAssignmentRule> Rules)
{
    public List<string> Validate(ReturnPlanAssignment item)
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

    public void RequireValid(ReturnPlanAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReturnPlanAssignment> items)
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

/// <summary>Functions of return plan assignment rules.</summary>
public static class ReturnPlanAssignmentRules
{
    public static ReturnPlanAssignmentValidator DefaultReturnPlanAssignmentValidator()
    {
        return new ReturnPlanAssignmentValidator(
            new List<IReturnPlanAssignmentRule>
            {
                new ReturnPlanAssignmentRequiredNameRule("return plan assignment needs a value"),
                new ReturnPlanAssignmentNameLengthRule(255),
                new ReturnPlanAssignmentCodeLengthRule(80),
                new ReturnPlanAssignmentRequiredSummaryRule("return plan assignment needs an entry"),
                new ReturnPlanAssignmentSalaryBandLengthRule(32),
                new ReturnPlanAssignmentSummaryLengthRule(3600),
            });
    }
}
