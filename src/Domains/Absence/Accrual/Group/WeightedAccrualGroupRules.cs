namespace Acme.Workforce.Absence.Accrual.Group;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a weighted accrual group must pass. Check returns its problems, or none.</summary>
public interface IWeightedAccrualGroupRule
{
    List<string> Check(WeightedAccrualGroup item);
}

public sealed record WeightedAccrualGroupRequiredGradeRule(string Message) : IWeightedAccrualGroupRule
{
    public List<string> Check(WeightedAccrualGroup item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedAccrualGroupAllowedCostCenterRule(List<string> Allowed) : IWeightedAccrualGroupRule
{
    public List<string> Check(WeightedAccrualGroup item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.CostCenter)))
        {
            return new List<string> { $"cost center {item.CostCenter} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record WeightedAccrualGroupVisibleGuardRule(int Limit) : IWeightedAccrualGroupRule
{
    public List<string> Check(WeightedAccrualGroup item)
    {
        if (item.Visible && item.GraceDays > Limit)
        {
            return new List<string> { $"grace days {item.GraceDays} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record WeightedAccrualGroupGradeLengthRule(int MaxLength) : IWeightedAccrualGroupRule
{
    public List<string> Check(WeightedAccrualGroup item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedAccrualGroupGraceDaysRangeRule(int Min, int Max) : IWeightedAccrualGroupRule
{
    public List<string> Check(WeightedAccrualGroup item)
    {
        var valueValue = item.GraceDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"grace days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"grace days {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a weighted accrual group.</summary>
public sealed record WeightedAccrualGroupValidator(List<IWeightedAccrualGroupRule> Rules)
{
    public List<string> Validate(WeightedAccrualGroup item)
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

    public void RequireValid(WeightedAccrualGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedAccrualGroup> items)
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

/// <summary>Functions of weighted accrual group rules.</summary>
public static class WeightedAccrualGroupRules
{
    public static WeightedAccrualGroupValidator DefaultWeightedAccrualGroupValidator()
    {
        return new WeightedAccrualGroupValidator(
            new List<IWeightedAccrualGroupRule>
            {
                new WeightedAccrualGroupRequiredGradeRule("weighted accrual group needs this field"),
                new WeightedAccrualGroupAllowedCostCenterRule(new List<string>()),
                new WeightedAccrualGroupVisibleGuardRule(120),
                new WeightedAccrualGroupGradeLengthRule(80),
                new WeightedAccrualGroupGraceDaysRangeRule(16, 86400),
            });
    }
}
