namespace Acme.Workforce.Workplace.Desk.Draft;

/// <summary>A rule a desk plan must pass. Check returns its problems, or none.</summary>
public interface IDeskPlanRule
{
    List<string> Check(DeskPlan item);
}

public sealed record DeskPlanReviewerLengthRule(int MaxLength) : IDeskPlanRule
{
    public List<string> Check(DeskPlan item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeskPlanTenureMonthsRangeRule(int Min, int Max) : IDeskPlanRule
{
    public List<string> Check(DeskPlan item)
    {
        var valueValue = item.TenureMonths;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"tenure months {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"tenure months {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DeskPlanCurrencyLengthRule(int MaxLength) : IDeskPlanRule
{
    public List<string> Check(DeskPlan item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a desk plan.</summary>
public sealed record DeskPlanValidator(List<IDeskPlanRule> Rules)
{
    public List<string> Validate(DeskPlan item)
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

    public void RequireValid(DeskPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeskPlan> items)
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

/// <summary>Functions of desk plan rules.</summary>
public static class DeskPlanRules
{
    public static DeskPlanValidator DefaultDeskPlanValidator()
    {
        return new DeskPlanValidator(
            new List<IDeskPlanRule>
            {
                new DeskPlanReviewerLengthRule(64),
                new DeskPlanTenureMonthsRangeRule(64, 120),
                new DeskPlanCurrencyLengthRule(86400),
            });
    }
}
