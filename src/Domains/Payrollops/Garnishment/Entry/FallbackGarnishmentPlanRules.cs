namespace Acme.Workforce.Payrollops.Garnishment.Entry;

/// <summary>A rule a fallback garnishment plan must pass. Check returns its problems, or none.</summary>
public interface IFallbackGarnishmentPlanRule
{
    List<string> Check(FallbackGarnishmentPlan item);
}

public sealed record FallbackGarnishmentPlanRequiredTitleRule(string Message) : IFallbackGarnishmentPlanRule
{
    public List<string> Check(FallbackGarnishmentPlan item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackGarnishmentPlanGradeLengthRule(int MaxLength) : IFallbackGarnishmentPlanRule
{
    public List<string> Check(FallbackGarnishmentPlan item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackGarnishmentPlanScoreRangeRule(double Min, double Max) : IFallbackGarnishmentPlanRule
{
    public List<string> Check(FallbackGarnishmentPlan item)
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

public sealed record FallbackGarnishmentPlanCodeLengthRule(int MaxLength) : IFallbackGarnishmentPlanRule
{
    public List<string> Check(FallbackGarnishmentPlan item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback garnishment plan.</summary>
public sealed record FallbackGarnishmentPlanValidator(List<IFallbackGarnishmentPlanRule> Rules)
{
    public List<string> Validate(FallbackGarnishmentPlan item)
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

    public void RequireValid(FallbackGarnishmentPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackGarnishmentPlan> items)
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

/// <summary>Functions of fallback garnishment plan rules.</summary>
public static class FallbackGarnishmentPlanRules
{
    public static FallbackGarnishmentPlanValidator DefaultFallbackGarnishmentPlanValidator()
    {
        return new FallbackGarnishmentPlanValidator(
            new List<IFallbackGarnishmentPlanRule>
            {
                new FallbackGarnishmentPlanRequiredTitleRule("fallback garnishment plan needs this field"),
                new FallbackGarnishmentPlanGradeLengthRule(120),
                new FallbackGarnishmentPlanScoreRangeRule(3.4, 225.4),
                new FallbackGarnishmentPlanCodeLengthRule(120),
            });
    }
}
