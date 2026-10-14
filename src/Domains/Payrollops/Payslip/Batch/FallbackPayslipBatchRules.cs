namespace Acme.Workforce.Payrollops.Payslip.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a fallback payslip batch must pass. Check returns its problems, or none.</summary>
public interface IFallbackPayslipBatchRule
{
    List<string> Check(FallbackPayslipBatch item);
}

public sealed record FallbackPayslipBatchCodePrefixRule(string Prefix) : IFallbackPayslipBatchRule
{
    public List<string> Check(FallbackPayslipBatch item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackPayslipBatchAllowedTitleRule(List<string> Allowed) : IFallbackPayslipBatchRule
{
    public List<string> Check(FallbackPayslipBatch item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record FallbackPayslipBatchRequiredCountryRule(string Message) : IFallbackPayslipBatchRule
{
    public List<string> Check(FallbackPayslipBatch item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackPayslipBatchRequiredNameRule(string Message) : IFallbackPayslipBatchRule
{
    public List<string> Check(FallbackPayslipBatch item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackPayslipBatchRateRangeRule(double Min, double Max) : IFallbackPayslipBatchRule
{
    public List<string> Check(FallbackPayslipBatch item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackPayslipBatchRequiredDescriptionRule(string Message) : IFallbackPayslipBatchRule
{
    public List<string> Check(FallbackPayslipBatch item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback payslip batch.</summary>
public sealed record FallbackPayslipBatchValidator(List<IFallbackPayslipBatchRule> Rules)
{
    public List<string> Validate(FallbackPayslipBatch item)
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

    public void RequireValid(FallbackPayslipBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackPayslipBatch> items)
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

/// <summary>Functions of fallback payslip batch rules.</summary>
public static class FallbackPayslipBatchRules
{
    public static FallbackPayslipBatchValidator DefaultFallbackPayslipBatchValidator()
    {
        return new FallbackPayslipBatchValidator(
            new List<IFallbackPayslipBatchRule>
            {
                new FallbackPayslipBatchCodePrefixRule("X"),
                new FallbackPayslipBatchAllowedTitleRule(new List<string> { "fallback", "manual", "primary" }),
                new FallbackPayslipBatchRequiredCountryRule("fallback payslip batch needs this field"),
                new FallbackPayslipBatchRequiredNameRule("fallback payslip batch needs an entry"),
                new FallbackPayslipBatchRateRangeRule(7.1, 1735.1),
                new FallbackPayslipBatchRequiredDescriptionRule("fallback payslip batch needs an entry"),
            });
    }
}
