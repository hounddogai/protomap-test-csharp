namespace Acme.Workforce.Payrollops.Deduction.Agreement;

/// <summary>A rule a lenient deduction agreement must pass. Check returns its problems, or none.</summary>
public interface ILenientDeductionAgreementRule
{
    List<string> Check(LenientDeductionAgreement item);
}

public sealed record LenientDeductionAgreementCodeLengthRule(int MaxLength) : ILenientDeductionAgreementRule
{
    public List<string> Check(LenientDeductionAgreement item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientDeductionAgreementRequiredTitleRule(string Message) : ILenientDeductionAgreementRule
{
    public List<string> Check(LenientDeductionAgreement item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientDeductionAgreementLocaleLengthRule(int MaxLength) : ILenientDeductionAgreementRule
{
    public List<string> Check(LenientDeductionAgreement item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientDeductionAgreementVisibleGuardRule(double Limit) : ILenientDeductionAgreementRule
{
    public List<string> Check(LenientDeductionAgreement item)
    {
        if (item.Visible && item.Amount > Limit)
        {
            return new List<string> { $"amount {item.Amount} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record LenientDeductionAgreementCodePrefixRule(string Prefix) : ILenientDeductionAgreementRule
{
    public List<string> Check(LenientDeductionAgreement item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LenientDeductionAgreementRequiredEmployeeIdRule(string Message) : ILenientDeductionAgreementRule
{
    public List<string> Check(LenientDeductionAgreement item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient deduction agreement.</summary>
public sealed record LenientDeductionAgreementValidator(List<ILenientDeductionAgreementRule> Rules)
{
    public List<string> Validate(LenientDeductionAgreement item)
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

    public void RequireValid(LenientDeductionAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientDeductionAgreement> items)
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

/// <summary>Functions of lenient deduction agreement rules.</summary>
public static class LenientDeductionAgreementRules
{
    public static LenientDeductionAgreementValidator DefaultLenientDeductionAgreementValidator()
    {
        return new LenientDeductionAgreementValidator(
            new List<ILenientDeductionAgreementRule>
            {
                new LenientDeductionAgreementCodeLengthRule(32),
                new LenientDeductionAgreementRequiredTitleRule("lenient deduction agreement needs an entry"),
                new LenientDeductionAgreementLocaleLengthRule(255),
                new LenientDeductionAgreementVisibleGuardRule(9.1),
                new LenientDeductionAgreementCodePrefixRule("OPS"),
                new LenientDeductionAgreementRequiredEmployeeIdRule("lenient deduction agreement needs this field"),
            });
    }
}
