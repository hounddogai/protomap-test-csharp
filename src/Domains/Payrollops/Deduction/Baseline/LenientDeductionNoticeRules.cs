namespace Acme.Workforce.Payrollops.Deduction.Baseline;

/// <summary>A rule a lenient deduction notice must pass. Check returns its problems, or none.</summary>
public interface ILenientDeductionNoticeRule
{
    List<string> Check(LenientDeductionNotice item);
}

public sealed record LenientDeductionNoticeRequiredCostCenterRule(string Message) : ILenientDeductionNoticeRule
{
    public List<string> Check(LenientDeductionNotice item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientDeductionNoticeTenureMonthsRangeRule(int Min, int Max) : ILenientDeductionNoticeRule
{
    public List<string> Check(LenientDeductionNotice item)
    {
        var valueValue = item.TenureMonths;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"tenure months {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientDeductionNoticeGradeLengthRule(int MaxLength) : ILenientDeductionNoticeRule
{
    public List<string> Check(LenientDeductionNotice item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientDeductionNoticeLabelLengthRule(int MaxLength) : ILenientDeductionNoticeRule
{
    public List<string> Check(LenientDeductionNotice item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientDeductionNoticeRequiredCodeRule(string Message) : ILenientDeductionNoticeRule
{
    public List<string> Check(LenientDeductionNotice item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient deduction notice.</summary>
public sealed record LenientDeductionNoticeValidator(List<ILenientDeductionNoticeRule> Rules)
{
    public List<string> Validate(LenientDeductionNotice item)
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

    public void RequireValid(LenientDeductionNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientDeductionNotice> items)
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

/// <summary>Functions of lenient deduction notice rules.</summary>
public static class LenientDeductionNoticeRules
{
    public static LenientDeductionNoticeValidator DefaultLenientDeductionNoticeValidator()
    {
        return new LenientDeductionNoticeValidator(
            new List<ILenientDeductionNoticeRule>
            {
                new LenientDeductionNoticeRequiredCostCenterRule("lenient deduction notice needs an entry"),
                new LenientDeductionNoticeTenureMonthsRangeRule(32, 120),
                new LenientDeductionNoticeGradeLengthRule(64),
                new LenientDeductionNoticeLabelLengthRule(16),
                new LenientDeductionNoticeRequiredCodeRule("lenient deduction notice needs a value"),
            });
    }
}
