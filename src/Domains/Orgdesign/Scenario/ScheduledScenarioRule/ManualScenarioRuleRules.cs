namespace Acme.Workforce.Orgdesign.Scenario.ScheduledScenarioRule;

/// <summary>A rule a manual scenario rule must pass. Check returns its problems, or none.</summary>
public interface IManualScenarioRuleRule
{
    List<string> Check(ManualScenarioRule item);
}

public sealed record ManualScenarioRuleRequiredTimezoneRule(string Message) : IManualScenarioRuleRule
{
    public List<string> Check(ManualScenarioRule item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualScenarioRuleCodeLengthRule(int MaxLength) : IManualScenarioRuleRule
{
    public List<string> Check(ManualScenarioRule item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualScenarioRuleAttemptsRangeRule(int Min, int Max) : IManualScenarioRuleRule
{
    public List<string> Check(ManualScenarioRule item)
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

public sealed record ManualScenarioRulePriorityRangeRule(int Min, int Max) : IManualScenarioRuleRule
{
    public List<string> Check(ManualScenarioRule item)
    {
        var valueValue = item.Priority;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"priority {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual scenario rule.</summary>
public sealed record ManualScenarioRuleValidator(List<IManualScenarioRuleRule> Rules)
{
    public List<string> Validate(ManualScenarioRule item)
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

    public void RequireValid(ManualScenarioRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualScenarioRule> items)
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

/// <summary>Functions of manual scenario rule rules.</summary>
public static class ManualScenarioRuleRules
{
    public static ManualScenarioRuleValidator DefaultManualScenarioRuleValidator()
    {
        return new ManualScenarioRuleValidator(
            new List<IManualScenarioRuleRule>
            {
                new ManualScenarioRuleRequiredTimezoneRule("manual scenario rule needs this field"),
                new ManualScenarioRuleCodeLengthRule(120),
                new ManualScenarioRuleAttemptsRangeRule(3600, 86400),
                new ManualScenarioRulePriorityRangeRule(32, 255),
            });
    }
}
