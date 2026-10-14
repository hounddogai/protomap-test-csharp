namespace Acme.Workforce.Payrollops.Deduction.Policy;

/// <summary>A rule a deduction estimate must pass. Check returns its problems, or none.</summary>
public interface IDeductionEstimateRule
{
    List<string> Check(DeductionEstimate item);
}

public sealed record DeductionEstimateRequiredBadgeRule(string Message) : IDeductionEstimateRule
{
    public List<string> Check(DeductionEstimate item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeductionEstimateCodeLengthRule(int MaxLength) : IDeductionEstimateRule
{
    public List<string> Check(DeductionEstimate item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeductionEstimateCodePrefixRule(string Prefix) : IDeductionEstimateRule
{
    public List<string> Check(DeductionEstimate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a deduction estimate.</summary>
public sealed record DeductionEstimateValidator(List<IDeductionEstimateRule> Rules)
{
    public List<string> Validate(DeductionEstimate item)
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

    public void RequireValid(DeductionEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeductionEstimate> items)
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

/// <summary>Functions of deduction estimate rules.</summary>
public static class DeductionEstimateRules
{
    public static DeductionEstimateValidator DefaultDeductionEstimateValidator()
    {
        return new DeductionEstimateValidator(
            new List<IDeductionEstimateRule>
            {
                new DeductionEstimateRequiredBadgeRule("deduction estimate needs an entry"),
                new DeductionEstimateCodeLengthRule(86400),
                new DeductionEstimateCodePrefixRule("REF"),
            });
    }
}
