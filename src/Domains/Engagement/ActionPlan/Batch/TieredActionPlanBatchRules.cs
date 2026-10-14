namespace Acme.Workforce.Engagement.ActionPlan.Batch;

/// <summary>A rule a tiered action plan batch must pass. Check returns its problems, or none.</summary>
public interface ITieredActionPlanBatchRule
{
    List<string> Check(TieredActionPlanBatch item);
}

public sealed record TieredActionPlanBatchLabelLengthRule(int MaxLength) : ITieredActionPlanBatchRule
{
    public List<string> Check(TieredActionPlanBatch item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredActionPlanBatchRatingRangeRule(double Min, double Max) : ITieredActionPlanBatchRule
{
    public List<string> Check(TieredActionPlanBatch item)
    {
        var valueValue = item.Rating;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rating {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rating {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TieredActionPlanBatchRequiredSkillRule(string Message) : ITieredActionPlanBatchRule
{
    public List<string> Check(TieredActionPlanBatch item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered action plan batch.</summary>
public sealed record TieredActionPlanBatchValidator(List<ITieredActionPlanBatchRule> Rules)
{
    public List<string> Validate(TieredActionPlanBatch item)
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

    public void RequireValid(TieredActionPlanBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredActionPlanBatch> items)
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

/// <summary>Functions of tiered action plan batch rules.</summary>
public static class TieredActionPlanBatchRules
{
    public static TieredActionPlanBatchValidator DefaultTieredActionPlanBatchValidator()
    {
        return new TieredActionPlanBatchValidator(
            new List<ITieredActionPlanBatchRule>
            {
                new TieredActionPlanBatchLabelLengthRule(80),
                new TieredActionPlanBatchRatingRangeRule(0.4, 2195.5),
                new TieredActionPlanBatchRequiredSkillRule("tiered action plan batch needs an entry"),
            });
    }
}
