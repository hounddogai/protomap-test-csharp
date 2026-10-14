namespace Acme.Workforce.Orgdesign.CostModel.Summary;

/// <summary>A rule a cost model summary must pass. Check returns its problems, or none.</summary>
public interface ICostModelSummaryRule
{
    List<string> Check(CostModelSummary item);
}

public sealed record CostModelSummaryRequiredLocaleRule(string Message) : ICostModelSummaryRule
{
    public List<string> Check(CostModelSummary item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CostModelSummaryTagsLimitRule(int MaxCount, List<string> Banned) : ICostModelSummaryRule
{
    public List<string> Check(CostModelSummary item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

public sealed record CostModelSummaryBatchSizeRangeRule(int Min, int Max) : ICostModelSummaryRule
{
    public List<string> Check(CostModelSummary item)
    {
        var valueValue = item.BatchSize;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"batch size {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"batch size {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CostModelSummaryRequiredCodeRule(string Message) : ICostModelSummaryRule
{
    public List<string> Check(CostModelSummary item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a cost model summary.</summary>
public sealed record CostModelSummaryValidator(List<ICostModelSummaryRule> Rules)
{
    public List<string> Validate(CostModelSummary item)
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

    public void RequireValid(CostModelSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CostModelSummary> items)
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

/// <summary>Functions of cost model summary rules.</summary>
public static class CostModelSummaryRules
{
    public static CostModelSummaryValidator DefaultCostModelSummaryValidator()
    {
        return new CostModelSummaryValidator(
            new List<ICostModelSummaryRule>
            {
                new CostModelSummaryRequiredLocaleRule("cost model summary needs an entry"),
                new CostModelSummaryTagsLimitRule(1000, new List<string> { "scheduled", "manual", "seasonal" }),
                new CostModelSummaryBatchSizeRangeRule(1000, 3600),
                new CostModelSummaryRequiredCodeRule("cost model summary needs a value"),
            });
    }
}
