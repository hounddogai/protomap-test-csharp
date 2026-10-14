namespace Acme.Workforce.Compensation.Adjustment.AdjustmentRule;

/// <summary>A rule a global adjustment rule must pass. Check returns its problems, or none.</summary>
public interface IGlobalAdjustmentRuleRule
{
    List<string> Check(GlobalAdjustmentRule item);
}

public sealed record GlobalAdjustmentRuleLabelLengthRule(int MaxLength) : IGlobalAdjustmentRuleRule
{
    public List<string> Check(GlobalAdjustmentRule item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalAdjustmentRuleRequiredCodeRule(string Message) : IGlobalAdjustmentRuleRule
{
    public List<string> Check(GlobalAdjustmentRule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalAdjustmentRuleAttemptsRangeRule(int Min, int Max) : IGlobalAdjustmentRuleRule
{
    public List<string> Check(GlobalAdjustmentRule item)
    {
        var valueValue = item.Attempts;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"attempts {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"attempts {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record GlobalAdjustmentRuleEndsAtRangeRule(int Min, int Max) : IGlobalAdjustmentRuleRule
{
    public List<string> Check(GlobalAdjustmentRule item)
    {
        var valueValue = item.EndsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ends at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ends at {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a global adjustment rule.</summary>
public sealed record GlobalAdjustmentRuleValidator(List<IGlobalAdjustmentRuleRule> Rules)
{
    public List<string> Validate(GlobalAdjustmentRule item)
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

    public void RequireValid(GlobalAdjustmentRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalAdjustmentRule> items)
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

/// <summary>Functions of global adjustment rule rules.</summary>
public static class GlobalAdjustmentRuleRules
{
    public static GlobalAdjustmentRuleValidator DefaultGlobalAdjustmentRuleValidator()
    {
        return new GlobalAdjustmentRuleValidator(
            new List<IGlobalAdjustmentRuleRule>
            {
                new GlobalAdjustmentRuleLabelLengthRule(3600),
                new GlobalAdjustmentRuleRequiredCodeRule("global adjustment rule needs this field"),
                new GlobalAdjustmentRuleAttemptsRangeRule(255, 1000),
                new GlobalAdjustmentRuleEndsAtRangeRule(32, 64),
            });
    }
}
