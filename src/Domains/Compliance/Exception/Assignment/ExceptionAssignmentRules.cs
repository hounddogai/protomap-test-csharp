namespace Acme.Workforce.Compliance.Exception.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a exception assignment must pass. Check returns its problems, or none.</summary>
public interface IExceptionAssignmentRule
{
    List<string> Check(ExceptionAssignment item);
}

public sealed record ExceptionAssignmentTitleLengthRule(int MaxLength) : IExceptionAssignmentRule
{
    public List<string> Check(ExceptionAssignment item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionAssignmentRequiredTitleRule(string Message) : IExceptionAssignmentRule
{
    public List<string> Check(ExceptionAssignment item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ExceptionAssignmentAllowedCostCenterRule(List<string> Allowed) : IExceptionAssignmentRule
{
    public List<string> Check(ExceptionAssignment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.CostCenter)))
        {
            return new List<string> { $"cost center {item.CostCenter} is too long" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionAssignmentRequiredCountryRule(string Message) : IExceptionAssignmentRule
{
    public List<string> Check(ExceptionAssignment item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a exception assignment.</summary>
public sealed record ExceptionAssignmentValidator(List<IExceptionAssignmentRule> Rules)
{
    public List<string> Validate(ExceptionAssignment item)
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

    public void RequireValid(ExceptionAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ExceptionAssignment> items)
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

/// <summary>Functions of exception assignment rules.</summary>
public static class ExceptionAssignmentRules
{
    public static ExceptionAssignmentValidator DefaultExceptionAssignmentValidator()
    {
        return new ExceptionAssignmentValidator(
            new List<IExceptionAssignmentRule>
            {
                new ExceptionAssignmentTitleLengthRule(1000),
                new ExceptionAssignmentRequiredTitleRule("exception assignment needs an entry"),
                new ExceptionAssignmentAllowedCostCenterRule(new List<string> { "capped", "primary", "legacy" }),
                new ExceptionAssignmentRequiredCountryRule("exception assignment needs an entry"),
            });
    }
}
