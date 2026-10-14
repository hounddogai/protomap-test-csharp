namespace Acme.Workforce.Orgdesign.Scenario.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scenario audit must pass. Check returns its problems, or none.</summary>
public interface IScenarioAuditRule
{
    List<string> Check(ScenarioAudit item);
}

public sealed record ScenarioAuditTitleLengthRule(int MaxLength) : IScenarioAuditRule
{
    public List<string> Check(ScenarioAudit item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioAuditCodePrefixRule(string Prefix) : IScenarioAuditRule
{
    public List<string> Check(ScenarioAudit item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioAuditCodeLengthRule(int MaxLength) : IScenarioAuditRule
{
    public List<string> Check(ScenarioAudit item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioAuditAllowedTitleRule(List<string> Allowed) : IScenarioAuditRule
{
    public List<string> Check(ScenarioAudit item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is not allowed" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scenario audit.</summary>
public sealed record ScenarioAuditValidator(List<IScenarioAuditRule> Rules)
{
    public List<string> Validate(ScenarioAudit item)
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

    public void RequireValid(ScenarioAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScenarioAudit> items)
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

/// <summary>Functions of scenario audit rules.</summary>
public static class ScenarioAuditRules
{
    public static ScenarioAuditValidator DefaultScenarioAuditValidator()
    {
        return new ScenarioAuditValidator(
            new List<IScenarioAuditRule>
            {
                new ScenarioAuditTitleLengthRule(64),
                new ScenarioAuditCodePrefixRule("ACM"),
                new ScenarioAuditCodeLengthRule(1000),
                new ScenarioAuditAllowedTitleRule(new List<string>()),
            });
    }
}
