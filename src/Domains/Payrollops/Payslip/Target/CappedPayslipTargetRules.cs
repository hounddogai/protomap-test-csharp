namespace Acme.Workforce.Payrollops.Payslip.Target;

/// <summary>A rule a capped payslip target must pass. Check returns its problems, or none.</summary>
public interface ICappedPayslipTargetRule
{
    List<string> Check(CappedPayslipTarget item);
}

public sealed record CappedPayslipTargetLabelLengthRule(int MaxLength) : ICappedPayslipTargetRule
{
    public List<string> Check(CappedPayslipTarget item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedPayslipTargetCodePrefixRule(string Prefix) : ICappedPayslipTargetRule
{
    public List<string> Check(CappedPayslipTarget item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CappedPayslipTargetRequiredTitleRule(string Message) : ICappedPayslipTargetRule
{
    public List<string> Check(CappedPayslipTarget item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped payslip target.</summary>
public sealed record CappedPayslipTargetValidator(List<ICappedPayslipTargetRule> Rules)
{
    public List<string> Validate(CappedPayslipTarget item)
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

    public void RequireValid(CappedPayslipTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedPayslipTarget> items)
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

/// <summary>Functions of capped payslip target rules.</summary>
public static class CappedPayslipTargetRules
{
    public static CappedPayslipTargetValidator DefaultCappedPayslipTargetValidator()
    {
        return new CappedPayslipTargetValidator(
            new List<ICappedPayslipTargetRule>
            {
                new CappedPayslipTargetLabelLengthRule(80),
                new CappedPayslipTargetCodePrefixRule("OPS"),
                new CappedPayslipTargetRequiredTitleRule("capped payslip target needs this field"),
            });
    }
}
