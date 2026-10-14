namespace Acme.Workforce.Payrollops.Payslip.Estimate;

/// <summary>A rule a payslip estimate must pass. Check returns its problems, or none.</summary>
public interface IPayslipEstimateRule
{
    List<string> Check(PayslipEstimate item);
}

public sealed record PayslipEstimateNameLengthRule(int MaxLength) : IPayslipEstimateRule
{
    public List<string> Check(PayslipEstimate item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipEstimateCodePrefixRule(string Prefix) : IPayslipEstimateRule
{
    public List<string> Check(PayslipEstimate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipEstimateActiveGuardRule(int Limit) : IPayslipEstimateRule
{
    public List<string> Check(PayslipEstimate item)
    {
        if (item.Active && item.ExpiresAt > Limit)
        {
            return new List<string> { $"expires at {item.ExpiresAt} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record PayslipEstimateRequiredChannelRule(string Message) : IPayslipEstimateRule
{
    public List<string> Check(PayslipEstimate item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a payslip estimate.</summary>
public sealed record PayslipEstimateValidator(List<IPayslipEstimateRule> Rules)
{
    public List<string> Validate(PayslipEstimate item)
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

    public void RequireValid(PayslipEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayslipEstimate> items)
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

/// <summary>Functions of payslip estimate rules.</summary>
public static class PayslipEstimateRules
{
    public static PayslipEstimateValidator DefaultPayslipEstimateValidator()
    {
        return new PayslipEstimateValidator(
            new List<IPayslipEstimateRule>
            {
                new PayslipEstimateNameLengthRule(64),
                new PayslipEstimateCodePrefixRule("X"),
                new PayslipEstimateActiveGuardRule(120),
                new PayslipEstimateRequiredChannelRule("payslip estimate needs a value"),
            });
    }
}
