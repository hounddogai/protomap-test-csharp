namespace Acme.Workforce.Absence.Accrual.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a lenient accrual assignment must pass. Check returns its problems, or none.</summary>
public interface ILenientAccrualAssignmentRule
{
    List<string> Check(LenientAccrualAssignment item);
}

public sealed record LenientAccrualAssignmentRequiredCodeRule(string Message) : ILenientAccrualAssignmentRule
{
    public List<string> Check(LenientAccrualAssignment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientAccrualAssignmentPhoneLengthRule(int MaxLength) : ILenientAccrualAssignmentRule
{
    public List<string> Check(LenientAccrualAssignment item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientAccrualAssignmentCodeLengthRule(int MaxLength) : ILenientAccrualAssignmentRule
{
    public List<string> Check(LenientAccrualAssignment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientAccrualAssignmentRequiredPhoneRule(string Message) : ILenientAccrualAssignmentRule
{
    public List<string> Check(LenientAccrualAssignment item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientAccrualAssignmentAllowedCodeRule(List<string> Allowed) : ILenientAccrualAssignmentRule
{
    public List<string> Check(LenientAccrualAssignment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is missing" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient accrual assignment.</summary>
public sealed record LenientAccrualAssignmentValidator(List<ILenientAccrualAssignmentRule> Rules)
{
    public List<string> Validate(LenientAccrualAssignment item)
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

    public void RequireValid(LenientAccrualAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientAccrualAssignment> items)
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

/// <summary>Functions of lenient accrual assignment rules.</summary>
public static class LenientAccrualAssignmentRules
{
    public static LenientAccrualAssignmentValidator DefaultLenientAccrualAssignmentValidator()
    {
        return new LenientAccrualAssignmentValidator(
            new List<ILenientAccrualAssignmentRule>
            {
                new LenientAccrualAssignmentRequiredCodeRule("lenient accrual assignment needs a value"),
                new LenientAccrualAssignmentPhoneLengthRule(255),
                new LenientAccrualAssignmentCodeLengthRule(16),
                new LenientAccrualAssignmentRequiredPhoneRule("lenient accrual assignment needs a value"),
                new LenientAccrualAssignmentAllowedCodeRule(new List<string> { "global" }),
            });
    }
}
