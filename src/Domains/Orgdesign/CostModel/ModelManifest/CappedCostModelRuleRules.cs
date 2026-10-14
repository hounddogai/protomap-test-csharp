namespace Acme.Workforce.Orgdesign.CostModel.ModelManifest;

/// <summary>A rule a capped cost model rule must pass. Check returns its problems, or none.</summary>
public interface ICappedCostModelRuleRule
{
    List<string> Check(CappedCostModelRule item);
}

public sealed record CappedCostModelRuleRequiredCodeRule(string Message) : ICappedCostModelRuleRule
{
    public List<string> Check(CappedCostModelRule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedCostModelRuleRatioRangeRule(double Min, double Max) : ICappedCostModelRuleRule
{
    public List<string> Check(CappedCostModelRule item)
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

public sealed record CappedCostModelRuleCategoryLengthRule(int MaxLength) : ICappedCostModelRuleRule
{
    public List<string> Check(CappedCostModelRule item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped cost model rule.</summary>
public sealed record CappedCostModelRuleValidator(List<ICappedCostModelRuleRule> Rules)
{
    public List<string> Validate(CappedCostModelRule item)
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

    public void RequireValid(CappedCostModelRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedCostModelRule> items)
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

/// <summary>Functions of capped cost model rule rules.</summary>
public static class CappedCostModelRuleRules
{
    public static CappedCostModelRuleValidator DefaultCappedCostModelRuleValidator()
    {
        return new CappedCostModelRuleValidator(
            new List<ICappedCostModelRuleRule>
            {
                new CappedCostModelRuleRequiredCodeRule("capped cost model rule needs a value"),
                new CappedCostModelRuleRatioRangeRule(1.4, 3537.3),
                new CappedCostModelRuleCategoryLengthRule(32),
            });
    }
}
