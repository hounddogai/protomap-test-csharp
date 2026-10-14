namespace Acme.Workforce.Payrollops.Payslip.PayslipAssignment;

/// <summary>A rule a fallback payslip digest must pass. Check returns its problems, or none.</summary>
public interface IFallbackPayslipDigestRule
{
    List<string> Check(FallbackPayslipDigest item);
}

public sealed record FallbackPayslipDigestCodePrefixRule(string Prefix) : IFallbackPayslipDigestRule
{
    public List<string> Check(FallbackPayslipDigest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackPayslipDigestMaximumRangeRule(double Min, double Max) : IFallbackPayslipDigestRule
{
    public List<string> Check(FallbackPayslipDigest item)
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

public sealed record FallbackPayslipDigestRequiredExternalRefRule(string Message) : IFallbackPayslipDigestRule
{
    public List<string> Check(FallbackPayslipDigest item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackPayslipDigestRetriesRangeRule(int Min, int Max) : IFallbackPayslipDigestRule
{
    public List<string> Check(FallbackPayslipDigest item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackPayslipDigestRatioRangeRule(double Min, double Max) : IFallbackPayslipDigestRule
{
    public List<string> Check(FallbackPayslipDigest item)
    {
        var valueValue = item.Ratio;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ratio {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ratio {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FallbackPayslipDigestSkillLengthRule(int MaxLength) : IFallbackPayslipDigestRule
{
    public List<string> Check(FallbackPayslipDigest item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback payslip digest.</summary>
public sealed record FallbackPayslipDigestValidator(List<IFallbackPayslipDigestRule> Rules)
{
    public List<string> Validate(FallbackPayslipDigest item)
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

    public void RequireValid(FallbackPayslipDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackPayslipDigest> items)
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

/// <summary>Functions of fallback payslip digest rules.</summary>
public static class FallbackPayslipDigestRules
{
    public static FallbackPayslipDigestValidator DefaultFallbackPayslipDigestValidator()
    {
        return new FallbackPayslipDigestValidator(
            new List<IFallbackPayslipDigestRule>
            {
                new FallbackPayslipDigestCodePrefixRule("X"),
                new FallbackPayslipDigestMaximumRangeRule(5.5, 2841.5),
                new FallbackPayslipDigestRequiredExternalRefRule("fallback payslip digest needs an entry"),
                new FallbackPayslipDigestRetriesRangeRule(32, 3600),
                new FallbackPayslipDigestRatioRangeRule(9.4, 3490.4),
                new FallbackPayslipDigestSkillLengthRule(120),
            });
    }
}
