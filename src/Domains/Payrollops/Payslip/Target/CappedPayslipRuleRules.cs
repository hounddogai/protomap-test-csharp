namespace Acme.Workforce.Payrollops.Payslip.Target;

/// <summary>A rule a capped payslip rule must pass. Check returns its problems, or none.</summary>
public interface ICappedPayslipRuleRule
{
    List<string> Check(CappedPayslipRule item);
}

public sealed record CappedPayslipRuleSalaryBandLengthRule(int MaxLength) : ICappedPayslipRuleRule
{
    public List<string> Check(CappedPayslipRule item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedPayslipRuleRequiredSalaryBandRule(string Message) : ICappedPayslipRuleRule
{
    public List<string> Check(CappedPayslipRule item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedPayslipRuleCodePrefixRule(string Prefix) : ICappedPayslipRuleRule
{
    public List<string> Check(CappedPayslipRule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped payslip rule.</summary>
public sealed record CappedPayslipRuleValidator(List<ICappedPayslipRuleRule> Rules)
{
    public List<string> Validate(CappedPayslipRule item)
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

    public void RequireValid(CappedPayslipRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedPayslipRule> items)
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

/// <summary>Functions of capped payslip rule rules.</summary>
public static class CappedPayslipRuleRules
{
    public static CappedPayslipRuleValidator DefaultCappedPayslipRuleValidator()
    {
        return new CappedPayslipRuleValidator(
            new List<ICappedPayslipRuleRule>
            {
                new CappedPayslipRuleSalaryBandLengthRule(80),
                new CappedPayslipRuleRequiredSalaryBandRule("capped payslip rule needs a value"),
                new CappedPayslipRuleCodePrefixRule("OPS"),
            });
    }
}
