namespace Acme.Workforce.Payrollops.Deduction.Policy;

/// <summary>A rule a deduction override must pass. Check returns its problems, or none.</summary>
public interface IDeductionOverrideRule
{
    List<string> Check(DeductionOverride item);
}

public sealed record DeductionOverrideUpdatedAtRangeRule(int Min, int Max) : IDeductionOverrideRule
{
    public List<string> Check(DeductionOverride item)
    {
        var valueValue = item.UpdatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"updated at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DeductionOverrideCodeLengthRule(int MaxLength) : IDeductionOverrideRule
{
    public List<string> Check(DeductionOverride item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeductionOverrideRatioRangeRule(double Min, double Max) : IDeductionOverrideRule
{
    public List<string> Check(DeductionOverride item)
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

/// <summary>Runs every rule of a deduction override.</summary>
public sealed record DeductionOverrideValidator(List<IDeductionOverrideRule> Rules)
{
    public List<string> Validate(DeductionOverride item)
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

    public void RequireValid(DeductionOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeductionOverride> items)
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

/// <summary>Functions of deduction override rules.</summary>
public static class DeductionOverrideRules
{
    public static DeductionOverrideValidator DefaultDeductionOverrideValidator()
    {
        return new DeductionOverrideValidator(
            new List<IDeductionOverrideRule>
            {
                new DeductionOverrideUpdatedAtRangeRule(80, 86400),
                new DeductionOverrideCodeLengthRule(120),
                new DeductionOverrideRatioRangeRule(7.2, 4323.7),
            });
    }
}
