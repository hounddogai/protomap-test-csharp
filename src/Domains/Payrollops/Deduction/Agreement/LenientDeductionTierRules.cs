namespace Acme.Workforce.Payrollops.Deduction.Agreement;

/// <summary>A rule a lenient deduction tier must pass. Check returns its problems, or none.</summary>
public interface ILenientDeductionTierRule
{
    List<string> Check(LenientDeductionTier item);
}

public sealed record LenientDeductionTierRequiredPhoneRule(string Message) : ILenientDeductionTierRule
{
    public List<string> Check(LenientDeductionTier item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientDeductionTierEmployeeIdLengthRule(int MaxLength) : ILenientDeductionTierRule
{
    public List<string> Check(LenientDeductionTier item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientDeductionTierRequiredCodeRule(string Message) : ILenientDeductionTierRule
{
    public List<string> Check(LenientDeductionTier item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientDeductionTierCodeLengthRule(int MaxLength) : ILenientDeductionTierRule
{
    public List<string> Check(LenientDeductionTier item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientDeductionTierPhoneLengthRule(int MaxLength) : ILenientDeductionTierRule
{
    public List<string> Check(LenientDeductionTier item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient deduction tier.</summary>
public sealed record LenientDeductionTierValidator(List<ILenientDeductionTierRule> Rules)
{
    public List<string> Validate(LenientDeductionTier item)
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

    public void RequireValid(LenientDeductionTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientDeductionTier> items)
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

/// <summary>Functions of lenient deduction tier rules.</summary>
public static class LenientDeductionTierRules
{
    public static LenientDeductionTierValidator DefaultLenientDeductionTierValidator()
    {
        return new LenientDeductionTierValidator(
            new List<ILenientDeductionTierRule>
            {
                new LenientDeductionTierRequiredPhoneRule("lenient deduction tier needs a value"),
                new LenientDeductionTierEmployeeIdLengthRule(16),
                new LenientDeductionTierRequiredCodeRule("lenient deduction tier needs an entry"),
                new LenientDeductionTierCodeLengthRule(86400),
                new LenientDeductionTierPhoneLengthRule(16),
            });
    }
}
