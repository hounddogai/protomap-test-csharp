namespace Acme.Workforce.Orgdesign.Scenario.ScenarioTemplate;

/// <summary>A rule a manual scenario audit must pass. Check returns its problems, or none.</summary>
public interface IManualScenarioAuditRule
{
    List<string> Check(ManualScenarioAudit item);
}

public sealed record ManualScenarioAuditRequiredCodeRule(string Message) : IManualScenarioAuditRule
{
    public List<string> Check(ManualScenarioAudit item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualScenarioAuditTitleLengthRule(int MaxLength) : IManualScenarioAuditRule
{
    public List<string> Check(ManualScenarioAudit item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualScenarioAuditCapacityRangeRule(int Min, int Max) : IManualScenarioAuditRule
{
    public List<string> Check(ManualScenarioAudit item)
    {
        var valueValue = item.Capacity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"capacity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ManualScenarioAuditLocaleLengthRule(int MaxLength) : IManualScenarioAuditRule
{
    public List<string> Check(ManualScenarioAudit item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual scenario audit.</summary>
public sealed record ManualScenarioAuditValidator(List<IManualScenarioAuditRule> Rules)
{
    public List<string> Validate(ManualScenarioAudit item)
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

    public void RequireValid(ManualScenarioAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualScenarioAudit> items)
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

/// <summary>Functions of manual scenario audit rules.</summary>
public static class ManualScenarioAuditRules
{
    public static ManualScenarioAuditValidator DefaultManualScenarioAuditValidator()
    {
        return new ManualScenarioAuditValidator(
            new List<IManualScenarioAuditRule>
            {
                new ManualScenarioAuditRequiredCodeRule("manual scenario audit needs this field"),
                new ManualScenarioAuditTitleLengthRule(1000),
                new ManualScenarioAuditCapacityRangeRule(1000, 86400),
                new ManualScenarioAuditLocaleLengthRule(80),
            });
    }
}
