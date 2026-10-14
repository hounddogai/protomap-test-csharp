namespace Acme.Workforce.Absence.LeaveRequest.RequestBudget;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a seasonal leave request budget must pass. Check returns its problems, or none.</summary>
public interface ISeasonalLeaveRequestBudgetRule
{
    List<string> Check(SeasonalLeaveRequestBudget item);
}

public sealed record SeasonalLeaveRequestBudgetAllowedCostCenterRule(List<string> Allowed) : ISeasonalLeaveRequestBudgetRule
{
    public List<string> Check(SeasonalLeaveRequestBudget item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.CostCenter)))
        {
            return new List<string> { $"cost center {item.CostCenter} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalLeaveRequestBudgetCategoryLengthRule(int MaxLength) : ISeasonalLeaveRequestBudgetRule
{
    public List<string> Check(SeasonalLeaveRequestBudget item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalLeaveRequestBudgetApprovedGuardRule(int Limit) : ISeasonalLeaveRequestBudgetRule
{
    public List<string> Check(SeasonalLeaveRequestBudget item)
    {
        if (item.Approved && item.Quantity > Limit)
        {
            return new List<string> { $"quantity {item.Quantity} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal leave request budget.</summary>
public sealed record SeasonalLeaveRequestBudgetValidator(List<ISeasonalLeaveRequestBudgetRule> Rules)
{
    public List<string> Validate(SeasonalLeaveRequestBudget item)
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

    public void RequireValid(SeasonalLeaveRequestBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalLeaveRequestBudget> items)
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

/// <summary>Functions of seasonal leave request budget rules.</summary>
public static class SeasonalLeaveRequestBudgetRules
{
    public static SeasonalLeaveRequestBudgetValidator DefaultSeasonalLeaveRequestBudgetValidator()
    {
        return new SeasonalLeaveRequestBudgetValidator(
            new List<ISeasonalLeaveRequestBudgetRule>
            {
                new SeasonalLeaveRequestBudgetAllowedCostCenterRule(
                    new List<string> { "strict", "capped", "fallback" }),
                new SeasonalLeaveRequestBudgetCategoryLengthRule(80),
                new SeasonalLeaveRequestBudgetApprovedGuardRule(3600),
            });
    }
}
