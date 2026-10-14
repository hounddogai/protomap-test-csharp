namespace Acme.Workforce.Assets.LicenseSeat.Assignment;

/// <summary>A rule a global license seat assignment must pass. Check returns its problems, or none.</summary>
public interface IGlobalLicenseSeatAssignmentRule
{
    List<string> Check(GlobalLicenseSeatAssignment item);
}

public sealed record GlobalLicenseSeatAssignmentRequiredCodeRule(string Message) : IGlobalLicenseSeatAssignmentRule
{
    public List<string> Check(GlobalLicenseSeatAssignment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalLicenseSeatAssignmentDepartmentLengthRule(int MaxLength) : IGlobalLicenseSeatAssignmentRule
{
    public List<string> Check(GlobalLicenseSeatAssignment item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalLicenseSeatAssignmentNameLengthRule(int MaxLength) : IGlobalLicenseSeatAssignmentRule
{
    public List<string> Check(GlobalLicenseSeatAssignment item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global license seat assignment.</summary>
public sealed record GlobalLicenseSeatAssignmentValidator(List<IGlobalLicenseSeatAssignmentRule> Rules)
{
    public List<string> Validate(GlobalLicenseSeatAssignment item)
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

    public void RequireValid(GlobalLicenseSeatAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalLicenseSeatAssignment> items)
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

/// <summary>Functions of global license seat assignment rules.</summary>
public static class GlobalLicenseSeatAssignmentRules
{
    public static GlobalLicenseSeatAssignmentValidator DefaultGlobalLicenseSeatAssignmentValidator()
    {
        return new GlobalLicenseSeatAssignmentValidator(
            new List<IGlobalLicenseSeatAssignmentRule>
            {
                new GlobalLicenseSeatAssignmentRequiredCodeRule("global license seat assignment needs this field"),
                new GlobalLicenseSeatAssignmentDepartmentLengthRule(255),
                new GlobalLicenseSeatAssignmentNameLengthRule(80),
            });
    }
}
