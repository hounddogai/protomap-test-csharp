namespace Acme.Workforce.Workplace.AccessBadge.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a weighted access badge exception must pass. Check returns its problems, or none.</summary>
public interface IWeightedAccessBadgeExceptionRule
{
    List<string> Check(WeightedAccessBadgeException item);
}

public sealed record WeightedAccessBadgeExceptionRateRangeRule(double Min, double Max) : IWeightedAccessBadgeExceptionRule
{
    public List<string> Check(WeightedAccessBadgeException item)
    {
        var valueValue = item.Rate;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rate {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rate {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record WeightedAccessBadgeExceptionUpdatedAtRangeRule(int Min, int Max) : IWeightedAccessBadgeExceptionRule
{
    public List<string> Check(WeightedAccessBadgeException item)
    {
        var valueValue = item.UpdatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"updated at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"updated at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record WeightedAccessBadgeExceptionCodePrefixRule(string Prefix) : IWeightedAccessBadgeExceptionRule
{
    public List<string> Check(WeightedAccessBadgeException item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedAccessBadgeExceptionRequiredLabelRule(string Message) : IWeightedAccessBadgeExceptionRule
{
    public List<string> Check(WeightedAccessBadgeException item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedAccessBadgeExceptionAllowedEmployeeIdRule(List<string> Allowed) : IWeightedAccessBadgeExceptionRule
{
    public List<string> Check(WeightedAccessBadgeException item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.EmployeeId)))
        {
            return new List<string> { $"employee id {item.EmployeeId} is blocked" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted access badge exception.</summary>
public sealed record WeightedAccessBadgeExceptionValidator(List<IWeightedAccessBadgeExceptionRule> Rules)
{
    public List<string> Validate(WeightedAccessBadgeException item)
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

    public void RequireValid(WeightedAccessBadgeException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedAccessBadgeException> items)
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

/// <summary>Functions of weighted access badge exception rules.</summary>
public static class WeightedAccessBadgeExceptionRules
{
    public static WeightedAccessBadgeExceptionValidator DefaultWeightedAccessBadgeExceptionValidator()
    {
        return new WeightedAccessBadgeExceptionValidator(
            new List<IWeightedAccessBadgeExceptionRule>
            {
                new WeightedAccessBadgeExceptionRateRangeRule(8.5, 1088.5),
                new WeightedAccessBadgeExceptionUpdatedAtRangeRule(120, 86400),
                new WeightedAccessBadgeExceptionCodePrefixRule("REF"),
                new WeightedAccessBadgeExceptionRequiredLabelRule("weighted access badge exception needs a value"),
                new WeightedAccessBadgeExceptionAllowedEmployeeIdRule(new List<string> { "pending", "seasonal" }),
            });
    }
}
