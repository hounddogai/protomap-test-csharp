namespace Acme.Workforce.Absence.Accrual.Slot;

/// <summary>A rule a accrual policy must pass. Check returns its problems, or none.</summary>
public interface IAccrualPolicyRule
{
    List<string> Check(AccrualPolicy item);
}

public sealed record AccrualPolicyVisibleGuardRule(int Limit) : IAccrualPolicyRule
{
    public List<string> Check(AccrualPolicy item)
    {
        if (item.Visible && item.BatchSize > Limit)
        {
            return new List<string> { $"batch size {item.BatchSize} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record AccrualPolicyBatchSizeRangeRule(int Min, int Max) : IAccrualPolicyRule
{
    public List<string> Check(AccrualPolicy item)
    {
        var valueValue = item.BatchSize;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"batch size {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualPolicyUnitLengthRule(int MaxLength) : IAccrualPolicyRule
{
    public List<string> Check(AccrualPolicy item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a accrual policy.</summary>
public sealed record AccrualPolicyValidator(List<IAccrualPolicyRule> Rules)
{
    public List<string> Validate(AccrualPolicy item)
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

    public void RequireValid(AccrualPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccrualPolicy> items)
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

/// <summary>Functions of accrual policy rules.</summary>
public static class AccrualPolicyRules
{
    public static AccrualPolicyValidator DefaultAccrualPolicyValidator()
    {
        return new AccrualPolicyValidator(
            new List<IAccrualPolicyRule>
            {
                new AccrualPolicyVisibleGuardRule(255),
                new AccrualPolicyBatchSizeRangeRule(64, 80),
                new AccrualPolicyUnitLengthRule(120),
            });
    }
}
