namespace Acme.Workforce.Payrollops.Payslip.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a payslip baseline must pass. Check returns its problems, or none.</summary>
public interface IPayslipBaselineRule
{
    List<string> Check(PayslipBaseline item);
}

public sealed record PayslipBaselineLocaleLengthRule(int MaxLength) : IPayslipBaselineRule
{
    public List<string> Check(PayslipBaseline item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipBaselineRequiredCodeRule(string Message) : IPayslipBaselineRule
{
    public List<string> Check(PayslipBaseline item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayslipBaselineTitleLengthRule(int MaxLength) : IPayslipBaselineRule
{
    public List<string> Check(PayslipBaseline item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipBaselineRatioRangeRule(double Min, double Max) : IPayslipBaselineRule
{
    public List<string> Check(PayslipBaseline item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipBaselineAllowedCodeRule(List<string> Allowed) : IPayslipBaselineRule
{
    public List<string> Check(PayslipBaseline item)
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

public sealed record PayslipBaselineHoursRangeRule(double Min, double Max) : IPayslipBaselineRule
{
    public List<string> Check(PayslipBaseline item)
    {
        var valueValue = item.Hours;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"hours {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"hours {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a payslip baseline.</summary>
public sealed record PayslipBaselineValidator(List<IPayslipBaselineRule> Rules)
{
    public List<string> Validate(PayslipBaseline item)
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

    public void RequireValid(PayslipBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayslipBaseline> items)
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

/// <summary>Functions of payslip baseline rules.</summary>
public static class PayslipBaselineRules
{
    public static PayslipBaselineValidator DefaultPayslipBaselineValidator()
    {
        return new PayslipBaselineValidator(
            new List<IPayslipBaselineRule>
            {
                new PayslipBaselineLocaleLengthRule(120),
                new PayslipBaselineRequiredCodeRule("payslip baseline needs this field"),
                new PayslipBaselineTitleLengthRule(255),
                new PayslipBaselineRatioRangeRule(3.8, 2987.0),
                new PayslipBaselineAllowedCodeRule(new List<string>()),
                new PayslipBaselineHoursRangeRule(6.8, 684.0),
            });
    }
}
