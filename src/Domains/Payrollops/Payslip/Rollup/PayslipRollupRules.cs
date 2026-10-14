namespace Acme.Workforce.Payrollops.Payslip.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a payslip rollup must pass. Check returns its problems, or none.</summary>
public interface IPayslipRollupRule
{
    List<string> Check(PayslipRollup item);
}

public sealed record PayslipRollupDiscountRangeRule(double Min, double Max) : IPayslipRollupRule
{
    public List<string> Check(PayslipRollup item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipRollupRequiredNameRule(string Message) : IPayslipRollupRule
{
    public List<string> Check(PayslipRollup item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayslipRollupCodePrefixRule(string Prefix) : IPayslipRollupRule
{
    public List<string> Check(PayslipRollup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipRollupAllowedUnitRule(List<string> Allowed) : IPayslipRollupRule
{
    public List<string> Check(PayslipRollup item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Unit)))
        {
            return new List<string> { $"unit {item.Unit} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record PayslipRollupRequiredCodeRule(string Message) : IPayslipRollupRule
{
    public List<string> Check(PayslipRollup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayslipRollupCodeLengthRule(int MaxLength) : IPayslipRollupRule
{
    public List<string> Check(PayslipRollup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a payslip rollup.</summary>
public sealed record PayslipRollupValidator(List<IPayslipRollupRule> Rules)
{
    public List<string> Validate(PayslipRollup item)
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

    public void RequireValid(PayslipRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayslipRollup> items)
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

/// <summary>Functions of payslip rollup rules.</summary>
public static class PayslipRollupRules
{
    public static PayslipRollupValidator DefaultPayslipRollupValidator()
    {
        return new PayslipRollupValidator(
            new List<IPayslipRollupRule>
            {
                new PayslipRollupDiscountRangeRule(7.2, 1661.5),
                new PayslipRollupRequiredNameRule("payslip rollup needs a value"),
                new PayslipRollupCodePrefixRule("ACM"),
                new PayslipRollupAllowedUnitRule(new List<string> { "scheduled", "regional", "rolling" }),
                new PayslipRollupRequiredCodeRule("payslip rollup needs this field"),
                new PayslipRollupCodeLengthRule(3600),
            });
    }
}
