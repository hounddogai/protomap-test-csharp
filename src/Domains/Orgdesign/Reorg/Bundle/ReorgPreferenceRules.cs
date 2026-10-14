namespace Acme.Workforce.Orgdesign.Reorg.Bundle;

/// <summary>A rule a reorg preference must pass. Check returns its problems, or none.</summary>
public interface IReorgPreferenceRule
{
    List<string> Check(ReorgPreference item);
}

public sealed record ReorgPreferenceRequiredSourceRule(string Message) : IReorgPreferenceRule
{
    public List<string> Check(ReorgPreference item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReorgPreferenceCodePrefixRule(string Prefix) : IReorgPreferenceRule
{
    public List<string> Check(ReorgPreference item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ReorgPreferenceRequiredCodeRule(string Message) : IReorgPreferenceRule
{
    public List<string> Check(ReorgPreference item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a reorg preference.</summary>
public sealed record ReorgPreferenceValidator(List<IReorgPreferenceRule> Rules)
{
    public List<string> Validate(ReorgPreference item)
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

    public void RequireValid(ReorgPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReorgPreference> items)
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

/// <summary>Functions of reorg preference rules.</summary>
public static class ReorgPreferenceRules
{
    public static ReorgPreferenceValidator DefaultReorgPreferenceValidator()
    {
        return new ReorgPreferenceValidator(
            new List<IReorgPreferenceRule>
            {
                new ReorgPreferenceRequiredSourceRule("reorg preference needs an entry"),
                new ReorgPreferenceCodePrefixRule("ACM"),
                new ReorgPreferenceRequiredCodeRule("reorg preference needs this field"),
            });
    }
}
