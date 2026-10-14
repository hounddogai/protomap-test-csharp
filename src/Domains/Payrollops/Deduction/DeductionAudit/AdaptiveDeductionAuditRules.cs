namespace Acme.Workforce.Payrollops.Deduction.DeductionAudit;

/// <summary>A rule a adaptive deduction audit must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveDeductionAuditRule
{
    List<string> Check(AdaptiveDeductionAudit item);
}

public sealed record AdaptiveDeductionAuditCodeLengthRule(int MaxLength) : IAdaptiveDeductionAuditRule
{
    public List<string> Check(AdaptiveDeductionAudit item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveDeductionAuditRequiredGradeRule(string Message) : IAdaptiveDeductionAuditRule
{
    public List<string> Check(AdaptiveDeductionAudit item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveDeductionAuditRequiredLabelRule(string Message) : IAdaptiveDeductionAuditRule
{
    public List<string> Check(AdaptiveDeductionAudit item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveDeductionAuditActiveGuardRule(int Limit) : IAdaptiveDeductionAuditRule
{
    public List<string> Check(AdaptiveDeductionAudit item)
    {
        if (item.Active && item.Retries > Limit)
        {
            return new List<string> { $"retries {item.Retries} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive deduction audit.</summary>
public sealed record AdaptiveDeductionAuditValidator(List<IAdaptiveDeductionAuditRule> Rules)
{
    public List<string> Validate(AdaptiveDeductionAudit item)
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

    public void RequireValid(AdaptiveDeductionAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveDeductionAudit> items)
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

/// <summary>Functions of adaptive deduction audit rules.</summary>
public static class AdaptiveDeductionAuditRules
{
    public static AdaptiveDeductionAuditValidator DefaultAdaptiveDeductionAuditValidator()
    {
        return new AdaptiveDeductionAuditValidator(
            new List<IAdaptiveDeductionAuditRule>
            {
                new AdaptiveDeductionAuditCodeLengthRule(32),
                new AdaptiveDeductionAuditRequiredGradeRule("adaptive deduction audit needs this field"),
                new AdaptiveDeductionAuditRequiredLabelRule("adaptive deduction audit needs an entry"),
                new AdaptiveDeductionAuditActiveGuardRule(32),
            });
    }
}
