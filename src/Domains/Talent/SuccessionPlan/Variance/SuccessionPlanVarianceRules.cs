namespace Acme.Workforce.Talent.SuccessionPlan.Variance;

/// <summary>A rule a succession plan variance must pass. Check returns its problems, or none.</summary>
public interface ISuccessionPlanVarianceRule
{
    List<string> Check(SuccessionPlanVariance item);
}

public sealed record SuccessionPlanVarianceExpiresAtRangeRule(int Min, int Max) : ISuccessionPlanVarianceRule
{
    public List<string> Check(SuccessionPlanVariance item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanVarianceEmployeeIdLengthRule(int MaxLength) : ISuccessionPlanVarianceRule
{
    public List<string> Check(SuccessionPlanVariance item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanVarianceNameLengthRule(int MaxLength) : ISuccessionPlanVarianceRule
{
    public List<string> Check(SuccessionPlanVariance item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanVarianceRequiredTitleRule(string Message) : ISuccessionPlanVarianceRule
{
    public List<string> Check(SuccessionPlanVariance item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a succession plan variance.</summary>
public sealed record SuccessionPlanVarianceValidator(List<ISuccessionPlanVarianceRule> Rules)
{
    public List<string> Validate(SuccessionPlanVariance item)
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

    public void RequireValid(SuccessionPlanVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SuccessionPlanVariance> items)
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

/// <summary>Functions of succession plan variance rules.</summary>
public static class SuccessionPlanVarianceRules
{
    public static SuccessionPlanVarianceValidator DefaultSuccessionPlanVarianceValidator()
    {
        return new SuccessionPlanVarianceValidator(
            new List<ISuccessionPlanVarianceRule>
            {
                new SuccessionPlanVarianceExpiresAtRangeRule(16, 3600),
                new SuccessionPlanVarianceEmployeeIdLengthRule(32),
                new SuccessionPlanVarianceNameLengthRule(255),
                new SuccessionPlanVarianceRequiredTitleRule("succession plan variance needs this field"),
            });
    }
}
