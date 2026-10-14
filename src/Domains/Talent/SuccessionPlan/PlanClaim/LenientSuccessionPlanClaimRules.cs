namespace Acme.Workforce.Talent.SuccessionPlan.PlanClaim;

/// <summary>A rule a lenient succession plan claim must pass. Check returns its problems, or none.</summary>
public interface ILenientSuccessionPlanClaimRule
{
    List<string> Check(LenientSuccessionPlanClaim item);
}

public sealed record LenientSuccessionPlanClaimRequiredCostCenterRule(string Message) : ILenientSuccessionPlanClaimRule
{
    public List<string> Check(LenientSuccessionPlanClaim item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientSuccessionPlanClaimRequiredLabelRule(string Message) : ILenientSuccessionPlanClaimRule
{
    public List<string> Check(LenientSuccessionPlanClaim item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientSuccessionPlanClaimLabelLengthRule(int MaxLength) : ILenientSuccessionPlanClaimRule
{
    public List<string> Check(LenientSuccessionPlanClaim item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientSuccessionPlanClaimCostCenterLengthRule(int MaxLength) : ILenientSuccessionPlanClaimRule
{
    public List<string> Check(LenientSuccessionPlanClaim item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientSuccessionPlanClaimRequiredSourceRule(string Message) : ILenientSuccessionPlanClaimRule
{
    public List<string> Check(LenientSuccessionPlanClaim item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientSuccessionPlanClaimLeaveDaysRangeRule(int Min, int Max) : ILenientSuccessionPlanClaimRule
{
    public List<string> Check(LenientSuccessionPlanClaim item)
    {
        var valueValue = item.LeaveDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"leave days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"leave days {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a lenient succession plan claim.</summary>
public sealed record LenientSuccessionPlanClaimValidator(List<ILenientSuccessionPlanClaimRule> Rules)
{
    public List<string> Validate(LenientSuccessionPlanClaim item)
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

    public void RequireValid(LenientSuccessionPlanClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientSuccessionPlanClaim> items)
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

/// <summary>Functions of lenient succession plan claim rules.</summary>
public static class LenientSuccessionPlanClaimRules
{
    public static LenientSuccessionPlanClaimValidator DefaultLenientSuccessionPlanClaimValidator()
    {
        return new LenientSuccessionPlanClaimValidator(
            new List<ILenientSuccessionPlanClaimRule>
            {
                new LenientSuccessionPlanClaimRequiredCostCenterRule(
                    "lenient succession plan claim needs this field"),
                new LenientSuccessionPlanClaimRequiredLabelRule("lenient succession plan claim needs a value"),
                new LenientSuccessionPlanClaimLabelLengthRule(64),
                new LenientSuccessionPlanClaimCostCenterLengthRule(32),
                new LenientSuccessionPlanClaimRequiredSourceRule("lenient succession plan claim needs an entry"),
                new LenientSuccessionPlanClaimLeaveDaysRangeRule(255, 255),
            });
    }
}
