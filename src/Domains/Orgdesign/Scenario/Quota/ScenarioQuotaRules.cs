namespace Acme.Workforce.Orgdesign.Scenario.Quota;

/// <summary>A rule a scenario quota must pass. Check returns its problems, or none.</summary>
public interface IScenarioQuotaRule
{
    List<string> Check(ScenarioQuota item);
}

public sealed record ScenarioQuotaBatchSizeRangeRule(int Min, int Max) : IScenarioQuotaRule
{
    public List<string> Check(ScenarioQuota item)
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

public sealed record ScenarioQuotaStartsAtRangeRule(int Min, int Max) : IScenarioQuotaRule
{
    public List<string> Check(ScenarioQuota item)
    {
        var valueValue = item.StartsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"starts at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"starts at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScenarioQuotaTitleLengthRule(int MaxLength) : IScenarioQuotaRule
{
    public List<string> Check(ScenarioQuota item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioQuotaCodeLengthRule(int MaxLength) : IScenarioQuotaRule
{
    public List<string> Check(ScenarioQuota item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioQuotaCodePrefixRule(string Prefix) : IScenarioQuotaRule
{
    public List<string> Check(ScenarioQuota item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scenario quota.</summary>
public sealed record ScenarioQuotaValidator(List<IScenarioQuotaRule> Rules)
{
    public List<string> Validate(ScenarioQuota item)
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

    public void RequireValid(ScenarioQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScenarioQuota> items)
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

/// <summary>Functions of scenario quota rules.</summary>
public static class ScenarioQuotaRules
{
    public static ScenarioQuotaValidator DefaultScenarioQuotaValidator()
    {
        return new ScenarioQuotaValidator(
            new List<IScenarioQuotaRule>
            {
                new ScenarioQuotaBatchSizeRangeRule(120, 3600),
                new ScenarioQuotaStartsAtRangeRule(80, 255),
                new ScenarioQuotaTitleLengthRule(120),
                new ScenarioQuotaCodeLengthRule(16),
                new ScenarioQuotaCodePrefixRule("X"),
            });
    }
}
