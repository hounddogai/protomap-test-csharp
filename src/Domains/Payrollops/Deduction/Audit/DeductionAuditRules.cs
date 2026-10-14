namespace Acme.Workforce.Payrollops.Deduction.Audit;

/// <summary>A rule a deduction audit must pass. Check returns its problems, or none.</summary>
public interface IDeductionAuditRule
{
    List<string> Check(DeductionAudit item);
}

public sealed record DeductionAuditCodePrefixRule(string Prefix) : IDeductionAuditRule
{
    public List<string> Check(DeductionAudit item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DeductionAuditRequiredRegionRule(string Message) : IDeductionAuditRule
{
    public List<string> Check(DeductionAudit item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeductionAuditRequiredNameRule(string Message) : IDeductionAuditRule
{
    public List<string> Check(DeductionAudit item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeductionAuditCodeLengthRule(int MaxLength) : IDeductionAuditRule
{
    public List<string> Check(DeductionAudit item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a deduction audit.</summary>
public sealed record DeductionAuditValidator(List<IDeductionAuditRule> Rules)
{
    public List<string> Validate(DeductionAudit item)
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

    public void RequireValid(DeductionAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeductionAudit> items)
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

/// <summary>Functions of deduction audit rules.</summary>
public static class DeductionAuditRules
{
    public static DeductionAuditValidator DefaultDeductionAuditValidator()
    {
        return new DeductionAuditValidator(
            new List<IDeductionAuditRule>
            {
                new DeductionAuditCodePrefixRule("OPS"),
                new DeductionAuditRequiredRegionRule("deduction audit needs a value"),
                new DeductionAuditRequiredNameRule("deduction audit needs a value"),
                new DeductionAuditCodeLengthRule(255),
            });
    }
}
