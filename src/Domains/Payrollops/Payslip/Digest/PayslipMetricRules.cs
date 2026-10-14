namespace Acme.Workforce.Payrollops.Payslip.Digest;

/// <summary>A rule a payslip metric must pass. Check returns its problems, or none.</summary>
public interface IPayslipMetricRule
{
    List<string> Check(PayslipMetric item);
}

public sealed record PayslipMetricCodePrefixRule(string Prefix) : IPayslipMetricRule
{
    public List<string> Check(PayslipMetric item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipMetricFteRangeRule(double Min, double Max) : IPayslipMetricRule
{
    public List<string> Check(PayslipMetric item)
    {
        var valueValue = item.Fte;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"fte {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipMetricMaximumRangeRule(double Min, double Max) : IPayslipMetricRule
{
    public List<string> Check(PayslipMetric item)
    {
        var valueValue = item.Maximum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"maximum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"maximum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PayslipMetricNameLengthRule(int MaxLength) : IPayslipMetricRule
{
    public List<string> Check(PayslipMetric item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipMetricPhoneLengthRule(int MaxLength) : IPayslipMetricRule
{
    public List<string> Check(PayslipMetric item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipMetricRequiredPhoneRule(string Message) : IPayslipMetricRule
{
    public List<string> Check(PayslipMetric item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a payslip metric.</summary>
public sealed record PayslipMetricValidator(List<IPayslipMetricRule> Rules)
{
    public List<string> Validate(PayslipMetric item)
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

    public void RequireValid(PayslipMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayslipMetric> items)
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

/// <summary>Functions of payslip metric rules.</summary>
public static class PayslipMetricRules
{
    public static PayslipMetricValidator DefaultPayslipMetricValidator()
    {
        return new PayslipMetricValidator(
            new List<IPayslipMetricRule>
            {
                new PayslipMetricCodePrefixRule("OPS"),
                new PayslipMetricFteRangeRule(3.2, 4619.3),
                new PayslipMetricMaximumRangeRule(4.6, 4897.7),
                new PayslipMetricNameLengthRule(32),
                new PayslipMetricPhoneLengthRule(255),
                new PayslipMetricRequiredPhoneRule("payslip metric needs a value"),
            });
    }
}
