namespace Acme.Workforce.Orgdesign.Scenario.ScenarioRule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scheduled scenario manifest must pass. Check returns its problems, or none.</summary>
public interface IScheduledScenarioManifestRule
{
    List<string> Check(ScheduledScenarioManifest item);
}

public sealed record ScheduledScenarioManifestExternalRefLengthRule(int MaxLength) : IScheduledScenarioManifestRule
{
    public List<string> Check(ScheduledScenarioManifest item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledScenarioManifestLimitRangeRule(int Min, int Max) : IScheduledScenarioManifestRule
{
    public List<string> Check(ScheduledScenarioManifest item)
    {
        var valueValue = item.Limit;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"limit {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledScenarioManifestAllowedExternalRefRule(List<string> Allowed) : IScheduledScenarioManifestRule
{
    public List<string> Check(ScheduledScenarioManifest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ExternalRef)))
        {
            return new List<string> { $"external ref {item.ExternalRef} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledScenarioManifestRequiredCodeRule(string Message) : IScheduledScenarioManifestRule
{
    public List<string> Check(ScheduledScenarioManifest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled scenario manifest.</summary>
public sealed record ScheduledScenarioManifestValidator(List<IScheduledScenarioManifestRule> Rules)
{
    public List<string> Validate(ScheduledScenarioManifest item)
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

    public void RequireValid(ScheduledScenarioManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledScenarioManifest> items)
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

/// <summary>Functions of scheduled scenario manifest rules.</summary>
public static class ScheduledScenarioManifestRules
{
    public static ScheduledScenarioManifestValidator DefaultScheduledScenarioManifestValidator()
    {
        return new ScheduledScenarioManifestValidator(
            new List<IScheduledScenarioManifestRule>
            {
                new ScheduledScenarioManifestExternalRefLengthRule(1000),
                new ScheduledScenarioManifestLimitRangeRule(80, 255),
                new ScheduledScenarioManifestAllowedExternalRefRule(new List<string>()),
                new ScheduledScenarioManifestRequiredCodeRule("scheduled scenario manifest needs an entry"),
            });
    }
}
