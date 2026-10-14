namespace Acme.Workforce.Orgdesign.Scenario.Audit;

/// <summary>A rule a legacy scenario override must pass. Check returns its problems, or none.</summary>
public interface ILegacyScenarioOverrideRule
{
    List<string> Check(LegacyScenarioOverride item);
}

public sealed record LegacyScenarioOverrideRequiredContactEmailRule(string Message) : ILegacyScenarioOverrideRule
{
    public List<string> Check(LegacyScenarioOverride item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyScenarioOverrideTitleLengthRule(int MaxLength) : ILegacyScenarioOverrideRule
{
    public List<string> Check(LegacyScenarioOverride item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyScenarioOverrideRequiredNotesRule(string Message) : ILegacyScenarioOverrideRule
{
    public List<string> Check(LegacyScenarioOverride item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy scenario override.</summary>
public sealed record LegacyScenarioOverrideValidator(List<ILegacyScenarioOverrideRule> Rules)
{
    public List<string> Validate(LegacyScenarioOverride item)
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

    public void RequireValid(LegacyScenarioOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyScenarioOverride> items)
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

/// <summary>Functions of legacy scenario override rules.</summary>
public static class LegacyScenarioOverrideRules
{
    public static LegacyScenarioOverrideValidator DefaultLegacyScenarioOverrideValidator()
    {
        return new LegacyScenarioOverrideValidator(
            new List<ILegacyScenarioOverrideRule>
            {
                new LegacyScenarioOverrideRequiredContactEmailRule("legacy scenario override needs a value"),
                new LegacyScenarioOverrideTitleLengthRule(64),
                new LegacyScenarioOverrideRequiredNotesRule("legacy scenario override needs an entry"),
            });
    }
}
