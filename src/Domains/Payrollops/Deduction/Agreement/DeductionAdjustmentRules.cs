namespace Acme.Workforce.Payrollops.Deduction.Agreement;

/// <summary>A rule a deduction adjustment must pass. Check returns its problems, or none.</summary>
public interface IDeductionAdjustmentRule
{
    List<string> Check(DeductionAdjustment item);
}

public sealed record DeductionAdjustmentLocationLengthRule(int MaxLength) : IDeductionAdjustmentRule
{
    public List<string> Check(DeductionAdjustment item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeductionAdjustmentRequiredSourceRule(string Message) : IDeductionAdjustmentRule
{
    public List<string> Check(DeductionAdjustment item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeductionAdjustmentScoreRangeRule(double Min, double Max) : IDeductionAdjustmentRule
{
    public List<string> Check(DeductionAdjustment item)
    {
        var valueValue = item.Score;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"score {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"score {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a deduction adjustment.</summary>
public sealed record DeductionAdjustmentValidator(List<IDeductionAdjustmentRule> Rules)
{
    public List<string> Validate(DeductionAdjustment item)
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

    public void RequireValid(DeductionAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeductionAdjustment> items)
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

/// <summary>Functions of deduction adjustment rules.</summary>
public static class DeductionAdjustmentRules
{
    public static DeductionAdjustmentValidator DefaultDeductionAdjustmentValidator()
    {
        return new DeductionAdjustmentValidator(
            new List<IDeductionAdjustmentRule>
            {
                new DeductionAdjustmentLocationLengthRule(16),
                new DeductionAdjustmentRequiredSourceRule("deduction adjustment needs this field"),
                new DeductionAdjustmentScoreRangeRule(9.1, 3680.4),
            });
    }
}
