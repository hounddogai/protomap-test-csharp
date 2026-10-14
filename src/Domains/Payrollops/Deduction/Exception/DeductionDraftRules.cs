namespace Acme.Workforce.Payrollops.Deduction.Exception;

/// <summary>A rule a deduction draft must pass. Check returns its problems, or none.</summary>
public interface IDeductionDraftRule
{
    List<string> Check(DeductionDraft item);
}

public sealed record DeductionDraftDepartmentLengthRule(int MaxLength) : IDeductionDraftRule
{
    public List<string> Check(DeductionDraft item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeductionDraftTitleLengthRule(int MaxLength) : IDeductionDraftRule
{
    public List<string> Check(DeductionDraft item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeductionDraftArchivedGuardRule(double Limit) : IDeductionDraftRule
{
    public List<string> Check(DeductionDraft item)
    {
        if (item.Archived && item.Ratio > Limit)
        {
            return new List<string> { $"ratio {item.Ratio} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record DeductionDraftRequiredTitleRule(string Message) : IDeductionDraftRule
{
    public List<string> Check(DeductionDraft item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a deduction draft.</summary>
public sealed record DeductionDraftValidator(List<IDeductionDraftRule> Rules)
{
    public List<string> Validate(DeductionDraft item)
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

    public void RequireValid(DeductionDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeductionDraft> items)
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

/// <summary>Functions of deduction draft rules.</summary>
public static class DeductionDraftRules
{
    public static DeductionDraftValidator DefaultDeductionDraftValidator()
    {
        return new DeductionDraftValidator(
            new List<IDeductionDraftRule>
            {
                new DeductionDraftDepartmentLengthRule(86400),
                new DeductionDraftTitleLengthRule(3600),
                new DeductionDraftArchivedGuardRule(5.4),
                new DeductionDraftRequiredTitleRule("deduction draft needs this field"),
            });
    }
}
