namespace Acme.Workforce.Payrollops.Payslip.Manifest;

/// <summary>A rule a payslip exception must pass. Check returns its problems, or none.</summary>
public interface IPayslipExceptionRule
{
    List<string> Check(PayslipException item);
}

public sealed record PayslipExceptionCostRangeRule(double Min, double Max) : IPayslipExceptionRule
{
    public List<string> Check(PayslipException item)
    {
        var valueValue = item.Cost;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"cost {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"cost {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PayslipExceptionAttemptsRangeRule(int Min, int Max) : IPayslipExceptionRule
{
    public List<string> Check(PayslipException item)
    {
        var valueValue = item.Attempts;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"attempts {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipExceptionRequiredCodeRule(string Message) : IPayslipExceptionRule
{
    public List<string> Check(PayslipException item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayslipExceptionChannelLengthRule(int MaxLength) : IPayslipExceptionRule
{
    public List<string> Check(PayslipException item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a payslip exception.</summary>
public sealed record PayslipExceptionValidator(List<IPayslipExceptionRule> Rules)
{
    public List<string> Validate(PayslipException item)
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

    public void RequireValid(PayslipException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayslipException> items)
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

/// <summary>Functions of payslip exception rules.</summary>
public static class PayslipExceptionRules
{
    public static PayslipExceptionValidator DefaultPayslipExceptionValidator()
    {
        return new PayslipExceptionValidator(
            new List<IPayslipExceptionRule>
            {
                new PayslipExceptionCostRangeRule(5.6, 2268.5),
                new PayslipExceptionAttemptsRangeRule(80, 120),
                new PayslipExceptionRequiredCodeRule("payslip exception needs this field"),
                new PayslipExceptionChannelLengthRule(3600),
            });
    }
}
