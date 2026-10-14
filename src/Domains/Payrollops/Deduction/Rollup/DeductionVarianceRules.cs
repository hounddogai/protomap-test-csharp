namespace Acme.Workforce.Payrollops.Deduction.Rollup;

/// <summary>A rule a deduction variance must pass. Check returns its problems, or none.</summary>
public interface IDeductionVarianceRule
{
    List<string> Check(DeductionVariance item);
}

public sealed record DeductionVarianceCodeLengthRule(int MaxLength) : IDeductionVarianceRule
{
    public List<string> Check(DeductionVariance item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeductionVarianceBudgetRangeRule(double Min, double Max) : IDeductionVarianceRule
{
    public List<string> Check(DeductionVariance item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DeductionVarianceThresholdRangeRule(double Min, double Max) : IDeductionVarianceRule
{
    public List<string> Check(DeductionVariance item)
    {
        var valueValue = item.Threshold;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"threshold {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"threshold {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DeductionVarianceCodePrefixRule(string Prefix) : IDeductionVarianceRule
{
    public List<string> Check(DeductionVariance item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DeductionVarianceSkillLengthRule(int MaxLength) : IDeductionVarianceRule
{
    public List<string> Check(DeductionVariance item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a deduction variance.</summary>
public sealed record DeductionVarianceValidator(List<IDeductionVarianceRule> Rules)
{
    public List<string> Validate(DeductionVariance item)
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

    public void RequireValid(DeductionVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeductionVariance> items)
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

/// <summary>Functions of deduction variance rules.</summary>
public static class DeductionVarianceRules
{
    public static DeductionVarianceValidator DefaultDeductionVarianceValidator()
    {
        return new DeductionVarianceValidator(
            new List<IDeductionVarianceRule>
            {
                new DeductionVarianceCodeLengthRule(64),
                new DeductionVarianceBudgetRangeRule(6.2, 94.8),
                new DeductionVarianceThresholdRangeRule(1.1, 446.5),
                new DeductionVarianceCodePrefixRule("REF"),
                new DeductionVarianceSkillLengthRule(3600),
            });
    }
}
