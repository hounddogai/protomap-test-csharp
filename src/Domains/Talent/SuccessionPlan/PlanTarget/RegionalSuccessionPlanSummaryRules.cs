namespace Acme.Workforce.Talent.SuccessionPlan.PlanTarget;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a regional succession plan summary must pass. Check returns its problems, or none.</summary>
public interface IRegionalSuccessionPlanSummaryRule
{
    List<string> Check(RegionalSuccessionPlanSummary item);
}

public sealed record RegionalSuccessionPlanSummaryMinimumRangeRule(double Min, double Max) : IRegionalSuccessionPlanSummaryRule
{
    public List<string> Check(RegionalSuccessionPlanSummary item)
    {
        var valueValue = item.Minimum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"minimum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"minimum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RegionalSuccessionPlanSummaryCapacityRangeRule(int Min, int Max) : IRegionalSuccessionPlanSummaryRule
{
    public List<string> Check(RegionalSuccessionPlanSummary item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RegionalSuccessionPlanSummaryJobCodeLengthRule(int MaxLength) : IRegionalSuccessionPlanSummaryRule
{
    public List<string> Check(RegionalSuccessionPlanSummary item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalSuccessionPlanSummaryAllowedJobCodeRule(List<string> Allowed) : IRegionalSuccessionPlanSummaryRule
{
    public List<string> Check(RegionalSuccessionPlanSummary item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.JobCode)))
        {
            return new List<string> { $"job code {item.JobCode} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record RegionalSuccessionPlanSummaryCodeLengthRule(int MaxLength) : IRegionalSuccessionPlanSummaryRule
{
    public List<string> Check(RegionalSuccessionPlanSummary item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional succession plan summary.</summary>
public sealed record RegionalSuccessionPlanSummaryValidator(List<IRegionalSuccessionPlanSummaryRule> Rules)
{
    public List<string> Validate(RegionalSuccessionPlanSummary item)
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

    public void RequireValid(RegionalSuccessionPlanSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalSuccessionPlanSummary> items)
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

/// <summary>Functions of regional succession plan summary rules.</summary>
public static class RegionalSuccessionPlanSummaryRules
{
    public static RegionalSuccessionPlanSummaryValidator DefaultRegionalSuccessionPlanSummaryValidator()
    {
        return new RegionalSuccessionPlanSummaryValidator(
            new List<IRegionalSuccessionPlanSummaryRule>
            {
                new RegionalSuccessionPlanSummaryMinimumRangeRule(5.6, 4363.9),
                new RegionalSuccessionPlanSummaryCapacityRangeRule(64, 64),
                new RegionalSuccessionPlanSummaryJobCodeLengthRule(86400),
                new RegionalSuccessionPlanSummaryAllowedJobCodeRule(new List<string> { "scheduled" }),
                new RegionalSuccessionPlanSummaryCodeLengthRule(32),
            });
    }
}
