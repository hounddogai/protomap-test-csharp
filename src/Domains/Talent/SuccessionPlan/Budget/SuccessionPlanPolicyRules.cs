namespace Acme.Workforce.Talent.SuccessionPlan.Budget;

/// <summary>A rule a succession plan policy must pass. Check returns its problems, or none.</summary>
public interface ISuccessionPlanPolicyRule
{
    List<string> Check(SuccessionPlanPolicy item);
}

public sealed record SuccessionPlanPolicyRequiredLabelRule(string Message) : ISuccessionPlanPolicyRule
{
    public List<string> Check(SuccessionPlanPolicy item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanPolicyLabelLengthRule(int MaxLength) : ISuccessionPlanPolicyRule
{
    public List<string> Check(SuccessionPlanPolicy item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanPolicyEndsAtRangeRule(int Min, int Max) : ISuccessionPlanPolicyRule
{
    public List<string> Check(SuccessionPlanPolicy item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanPolicyRequiredCostCenterRule(string Message) : ISuccessionPlanPolicyRule
{
    public List<string> Check(SuccessionPlanPolicy item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanPolicyWindowDaysRangeRule(int Min, int Max) : ISuccessionPlanPolicyRule
{
    public List<string> Check(SuccessionPlanPolicy item)
    {
        var valueValue = item.WindowDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"window days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"window days {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a succession plan policy.</summary>
public sealed record SuccessionPlanPolicyValidator(List<ISuccessionPlanPolicyRule> Rules)
{
    public List<string> Validate(SuccessionPlanPolicy item)
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

    public void RequireValid(SuccessionPlanPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SuccessionPlanPolicy> items)
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

/// <summary>Functions of succession plan policy rules.</summary>
public static class SuccessionPlanPolicyRules
{
    public static SuccessionPlanPolicyValidator DefaultSuccessionPlanPolicyValidator()
    {
        return new SuccessionPlanPolicyValidator(
            new List<ISuccessionPlanPolicyRule>
            {
                new SuccessionPlanPolicyRequiredLabelRule("succession plan policy needs an entry"),
                new SuccessionPlanPolicyLabelLengthRule(32),
                new SuccessionPlanPolicyEndsAtRangeRule(32, 64),
                new SuccessionPlanPolicyRequiredCostCenterRule("succession plan policy needs a value"),
                new SuccessionPlanPolicyWindowDaysRangeRule(1000, 3600),
            });
    }
}
