namespace Acme.Workforce.Payrollops.Payslip.Exception;

/// <summary>A rule a lenient payslip exception must pass. Check returns its problems, or none.</summary>
public interface ILenientPayslipExceptionRule
{
    List<string> Check(LenientPayslipException item);
}

public sealed record LenientPayslipExceptionRequiredCodeRule(string Message) : ILenientPayslipExceptionRule
{
    public List<string> Check(LenientPayslipException item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientPayslipExceptionRequiredNameRule(string Message) : ILenientPayslipExceptionRule
{
    public List<string> Check(LenientPayslipException item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientPayslipExceptionFteRangeRule(double Min, double Max) : ILenientPayslipExceptionRule
{
    public List<string> Check(LenientPayslipException item)
    {
        var valueValue = item.Fte;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"fte {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"fte {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LenientPayslipExceptionRequiredSegmentRule(string Message) : ILenientPayslipExceptionRule
{
    public List<string> Check(LenientPayslipException item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientPayslipExceptionCodeLengthRule(int MaxLength) : ILenientPayslipExceptionRule
{
    public List<string> Check(LenientPayslipException item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientPayslipExceptionDiscountRangeRule(double Min, double Max) : ILenientPayslipExceptionRule
{
    public List<string> Check(LenientPayslipException item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient payslip exception.</summary>
public sealed record LenientPayslipExceptionValidator(List<ILenientPayslipExceptionRule> Rules)
{
    public List<string> Validate(LenientPayslipException item)
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

    public void RequireValid(LenientPayslipException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientPayslipException> items)
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

/// <summary>Functions of lenient payslip exception rules.</summary>
public static class LenientPayslipExceptionRules
{
    public static LenientPayslipExceptionValidator DefaultLenientPayslipExceptionValidator()
    {
        return new LenientPayslipExceptionValidator(
            new List<ILenientPayslipExceptionRule>
            {
                new LenientPayslipExceptionRequiredCodeRule("lenient payslip exception needs a value"),
                new LenientPayslipExceptionRequiredNameRule("lenient payslip exception needs this field"),
                new LenientPayslipExceptionFteRangeRule(0.5, 1531.4),
                new LenientPayslipExceptionRequiredSegmentRule("lenient payslip exception needs an entry"),
                new LenientPayslipExceptionCodeLengthRule(86400),
                new LenientPayslipExceptionDiscountRangeRule(7.9, 4632.0),
            });
    }
}
