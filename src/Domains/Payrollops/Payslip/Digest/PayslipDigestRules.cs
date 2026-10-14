namespace Acme.Workforce.Payrollops.Payslip.Digest;

/// <summary>A rule a payslip digest must pass. Check returns its problems, or none.</summary>
public interface IPayslipDigestRule
{
    List<string> Check(PayslipDigest item);
}

public sealed record PayslipDigestApprovedGuardRule(int Limit) : IPayslipDigestRule
{
    public List<string> Check(PayslipDigest item)
    {
        if (item.Approved && item.UpdatedAt > Limit)
        {
            return new List<string> { $"updated at {item.UpdatedAt} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record PayslipDigestUpdatedAtRangeRule(int Min, int Max) : IPayslipDigestRule
{
    public List<string> Check(PayslipDigest item)
    {
        var valueValue = item.UpdatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"updated at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipDigestLabelLengthRule(int MaxLength) : IPayslipDigestRule
{
    public List<string> Check(PayslipDigest item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipDigestSalaryBandLengthRule(int MaxLength) : IPayslipDigestRule
{
    public List<string> Check(PayslipDigest item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipDigestRequiredCodeRule(string Message) : IPayslipDigestRule
{
    public List<string> Check(PayslipDigest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a payslip digest.</summary>
public sealed record PayslipDigestValidator(List<IPayslipDigestRule> Rules)
{
    public List<string> Validate(PayslipDigest item)
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

    public void RequireValid(PayslipDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayslipDigest> items)
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

/// <summary>Functions of payslip digest rules.</summary>
public static class PayslipDigestRules
{
    public static PayslipDigestValidator DefaultPayslipDigestValidator()
    {
        return new PayslipDigestValidator(
            new List<IPayslipDigestRule>
            {
                new PayslipDigestApprovedGuardRule(120),
                new PayslipDigestUpdatedAtRangeRule(16, 255),
                new PayslipDigestLabelLengthRule(80),
                new PayslipDigestSalaryBandLengthRule(255),
                new PayslipDigestRequiredCodeRule("payslip digest needs a value"),
            });
    }
}
