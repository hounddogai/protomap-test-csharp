namespace Acme.Workforce.Orgdesign.Scenario.Cycle;

/// <summary>A rule a scenario manifest must pass. Check returns its problems, or none.</summary>
public interface IScenarioManifestRule
{
    List<string> Check(ScenarioManifest item);
}

public sealed record ScenarioManifestCodePrefixRule(string Prefix) : IScenarioManifestRule
{
    public List<string> Check(ScenarioManifest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioManifestRequiredNameRule(string Message) : IScenarioManifestRule
{
    public List<string> Check(ScenarioManifest item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScenarioManifestRequiredNotesRule(string Message) : IScenarioManifestRule
{
    public List<string> Check(ScenarioManifest item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScenarioManifestNameLengthRule(int MaxLength) : IScenarioManifestRule
{
    public List<string> Check(ScenarioManifest item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scenario manifest.</summary>
public sealed record ScenarioManifestValidator(List<IScenarioManifestRule> Rules)
{
    public List<string> Validate(ScenarioManifest item)
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

    public void RequireValid(ScenarioManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScenarioManifest> items)
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

/// <summary>Functions of scenario manifest rules.</summary>
public static class ScenarioManifestRules
{
    public static ScenarioManifestValidator DefaultScenarioManifestValidator()
    {
        return new ScenarioManifestValidator(
            new List<IScenarioManifestRule>
            {
                new ScenarioManifestCodePrefixRule("Q"),
                new ScenarioManifestRequiredNameRule("scenario manifest needs a value"),
                new ScenarioManifestRequiredNotesRule("scenario manifest needs an entry"),
                new ScenarioManifestNameLengthRule(120),
            });
    }
}
