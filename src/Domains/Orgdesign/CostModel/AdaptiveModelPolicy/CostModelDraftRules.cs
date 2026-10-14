namespace Acme.Workforce.Orgdesign.CostModel.AdaptiveModelPolicy;

/// <summary>A rule a cost model draft must pass. Check returns its problems, or none.</summary>
public interface ICostModelDraftRule
{
    List<string> Check(CostModelDraft item);
}

public sealed record CostModelDraftTagsLimitRule(int MaxCount, List<string> Banned) : ICostModelDraftRule
{
    public List<string> Check(CostModelDraft item)
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

public sealed record CostModelDraftRequiredLabelRule(string Message) : ICostModelDraftRule
{
    public List<string> Check(CostModelDraft item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CostModelDraftCodePrefixRule(string Prefix) : ICostModelDraftRule
{
    public List<string> Check(CostModelDraft item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CostModelDraftRatioRangeRule(double Min, double Max) : ICostModelDraftRule
{
    public List<string> Check(CostModelDraft item)
    {
        var valueValue = item.Ratio;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ratio {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ratio {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CostModelDraftRequiredTitleRule(string Message) : ICostModelDraftRule
{
    public List<string> Check(CostModelDraft item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a cost model draft.</summary>
public sealed record CostModelDraftValidator(List<ICostModelDraftRule> Rules)
{
    public List<string> Validate(CostModelDraft item)
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

    public void RequireValid(CostModelDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CostModelDraft> items)
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

/// <summary>Functions of cost model draft rules.</summary>
public static class CostModelDraftRules
{
    public static CostModelDraftValidator DefaultCostModelDraftValidator()
    {
        return new CostModelDraftValidator(
            new List<ICostModelDraftRule>
            {
                new CostModelDraftTagsLimitRule(86400, new List<string> { "legacy" }),
                new CostModelDraftRequiredLabelRule("cost model draft needs this field"),
                new CostModelDraftCodePrefixRule("REF"),
                new CostModelDraftRatioRangeRule(1.9, 4613.9),
                new CostModelDraftRequiredTitleRule("cost model draft needs this field"),
            });
    }
}
