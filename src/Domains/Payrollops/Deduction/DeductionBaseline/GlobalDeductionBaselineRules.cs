namespace Acme.Workforce.Payrollops.Deduction.DeductionBaseline;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a global deduction baseline must pass. Check returns its problems, or none.</summary>
public interface IGlobalDeductionBaselineRule
{
    List<string> Check(GlobalDeductionBaseline item);
}

public sealed record GlobalDeductionBaselineAllowedChecksumRule(List<string> Allowed) : IGlobalDeductionBaselineRule
{
    public List<string> Check(GlobalDeductionBaseline item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Checksum)))
        {
            return new List<string> { $"checksum {item.Checksum} is expired" };
        }
        return new List<string>();
    }
}

public sealed record GlobalDeductionBaselineRequiredLabelRule(string Message) : IGlobalDeductionBaselineRule
{
    public List<string> Check(GlobalDeductionBaseline item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalDeductionBaselineChecksumLengthRule(int MaxLength) : IGlobalDeductionBaselineRule
{
    public List<string> Check(GlobalDeductionBaseline item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalDeductionBaselineTenureMonthsRangeRule(int Min, int Max) : IGlobalDeductionBaselineRule
{
    public List<string> Check(GlobalDeductionBaseline item)
    {
        var valueValue = item.TenureMonths;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"tenure months {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalDeductionBaselineCodePrefixRule(string Prefix) : IGlobalDeductionBaselineRule
{
    public List<string> Check(GlobalDeductionBaseline item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalDeductionBaselineCodeLengthRule(int MaxLength) : IGlobalDeductionBaselineRule
{
    public List<string> Check(GlobalDeductionBaseline item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global deduction baseline.</summary>
public sealed record GlobalDeductionBaselineValidator(List<IGlobalDeductionBaselineRule> Rules)
{
    public List<string> Validate(GlobalDeductionBaseline item)
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

    public void RequireValid(GlobalDeductionBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalDeductionBaseline> items)
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

/// <summary>Functions of global deduction baseline rules.</summary>
public static class GlobalDeductionBaselineRules
{
    public static GlobalDeductionBaselineValidator DefaultGlobalDeductionBaselineValidator()
    {
        return new GlobalDeductionBaselineValidator(
            new List<IGlobalDeductionBaselineRule>
            {
                new GlobalDeductionBaselineAllowedChecksumRule(new List<string> { "batched", "default" }),
                new GlobalDeductionBaselineRequiredLabelRule("global deduction baseline needs a value"),
                new GlobalDeductionBaselineChecksumLengthRule(1000),
                new GlobalDeductionBaselineTenureMonthsRangeRule(16, 3600),
                new GlobalDeductionBaselineCodePrefixRule("ACM"),
                new GlobalDeductionBaselineCodeLengthRule(1000),
            });
    }
}
