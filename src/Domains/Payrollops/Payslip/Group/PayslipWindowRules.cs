namespace Acme.Workforce.Payrollops.Payslip.Group;

/// <summary>A rule a payslip window must pass. Check returns its problems, or none.</summary>
public interface IPayslipWindowRule
{
    List<string> Check(PayslipWindow item);
}

public sealed record PayslipWindowRequiredCodeRule(string Message) : IPayslipWindowRule
{
    public List<string> Check(PayslipWindow item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayslipWindowSummaryLengthRule(int MaxLength) : IPayslipWindowRule
{
    public List<string> Check(PayslipWindow item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipWindowRequiredLocationRule(string Message) : IPayslipWindowRule
{
    public List<string> Check(PayslipWindow item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayslipWindowCeilingRangeRule(double Min, double Max) : IPayslipWindowRule
{
    public List<string> Check(PayslipWindow item)
    {
        var valueValue = item.Ceiling;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ceiling {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipWindowRequiredNameRule(string Message) : IPayslipWindowRule
{
    public List<string> Check(PayslipWindow item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a payslip window.</summary>
public sealed record PayslipWindowValidator(List<IPayslipWindowRule> Rules)
{
    public List<string> Validate(PayslipWindow item)
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

    public void RequireValid(PayslipWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayslipWindow> items)
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

/// <summary>Functions of payslip window rules.</summary>
public static class PayslipWindowRules
{
    public static PayslipWindowValidator DefaultPayslipWindowValidator()
    {
        return new PayslipWindowValidator(
            new List<IPayslipWindowRule>
            {
                new PayslipWindowRequiredCodeRule("payslip window needs this field"),
                new PayslipWindowSummaryLengthRule(255),
                new PayslipWindowRequiredLocationRule("payslip window needs an entry"),
                new PayslipWindowCeilingRangeRule(3.3, 3596.5),
                new PayslipWindowRequiredNameRule("payslip window needs a value"),
            });
    }
}
