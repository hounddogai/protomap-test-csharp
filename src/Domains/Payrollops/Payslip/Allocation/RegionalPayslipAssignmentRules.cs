namespace Acme.Workforce.Payrollops.Payslip.Allocation;

/// <summary>A rule a regional payslip assignment must pass. Check returns its problems, or none.</summary>
public interface IRegionalPayslipAssignmentRule
{
    List<string> Check(RegionalPayslipAssignment item);
}

public sealed record RegionalPayslipAssignmentLocaleLengthRule(int MaxLength) : IRegionalPayslipAssignmentRule
{
    public List<string> Check(RegionalPayslipAssignment item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalPayslipAssignmentSequenceRangeRule(int Min, int Max) : IRegionalPayslipAssignmentRule
{
    public List<string> Check(RegionalPayslipAssignment item)
    {
        var valueValue = item.Sequence;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"sequence {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"sequence {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RegionalPayslipAssignmentRequiredPhoneRule(string Message) : IRegionalPayslipAssignmentRule
{
    public List<string> Check(RegionalPayslipAssignment item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional payslip assignment.</summary>
public sealed record RegionalPayslipAssignmentValidator(List<IRegionalPayslipAssignmentRule> Rules)
{
    public List<string> Validate(RegionalPayslipAssignment item)
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

    public void RequireValid(RegionalPayslipAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalPayslipAssignment> items)
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

/// <summary>Functions of regional payslip assignment rules.</summary>
public static class RegionalPayslipAssignmentRules
{
    public static RegionalPayslipAssignmentValidator DefaultRegionalPayslipAssignmentValidator()
    {
        return new RegionalPayslipAssignmentValidator(
            new List<IRegionalPayslipAssignmentRule>
            {
                new RegionalPayslipAssignmentLocaleLengthRule(255),
                new RegionalPayslipAssignmentSequenceRangeRule(80, 255),
                new RegionalPayslipAssignmentRequiredPhoneRule("regional payslip assignment needs a value"),
            });
    }
}
