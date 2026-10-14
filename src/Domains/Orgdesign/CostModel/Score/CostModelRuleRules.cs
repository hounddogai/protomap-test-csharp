namespace Acme.Workforce.Orgdesign.CostModel.Score;

/// <summary>A rule a cost model rule must pass. Check returns its problems, or none.</summary>
public interface ICostModelRuleRule
{
    List<string> Check(CostModelRule item);
}

public sealed record CostModelRuleRequiredSourceRule(string Message) : ICostModelRuleRule
{
    public List<string> Check(CostModelRule item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CostModelRuleCodeLengthRule(int MaxLength) : ICostModelRuleRule
{
    public List<string> Check(CostModelRule item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CostModelRuleTagsLimitRule(int MaxCount, List<string> Banned) : ICostModelRuleRule
{
    public List<string> Check(CostModelRule item)
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

/// <summary>Runs every rule of a cost model rule.</summary>
public sealed record CostModelRuleValidator(List<ICostModelRuleRule> Rules)
{
    public List<string> Validate(CostModelRule item)
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

    public void RequireValid(CostModelRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CostModelRule> items)
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

/// <summary>Functions of cost model rule rules.</summary>
public static class CostModelRuleRules
{
    public static CostModelRuleValidator DefaultCostModelRuleValidator()
    {
        return new CostModelRuleValidator(
            new List<ICostModelRuleRule>
            {
                new CostModelRuleRequiredSourceRule("cost model rule needs a value"),
                new CostModelRuleCodeLengthRule(64),
                new CostModelRuleTagsLimitRule(80, new List<string> { "rolling", "default" }),
            });
    }
}
