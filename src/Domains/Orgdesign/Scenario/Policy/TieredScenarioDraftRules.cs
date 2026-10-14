namespace Acme.Workforce.Orgdesign.Scenario.Policy;

/// <summary>A rule a tiered scenario draft must pass. Check returns its problems, or none.</summary>
public interface ITieredScenarioDraftRule
{
    List<string> Check(TieredScenarioDraft item);
}

public sealed record TieredScenarioDraftLabelLengthRule(int MaxLength) : ITieredScenarioDraftRule
{
    public List<string> Check(TieredScenarioDraft item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredScenarioDraftSequenceRangeRule(int Min, int Max) : ITieredScenarioDraftRule
{
    public List<string> Check(TieredScenarioDraft item)
    {
        var valueValue = item.Sequence;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"sequence {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"sequence {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TieredScenarioDraftCodePrefixRule(string Prefix) : ITieredScenarioDraftRule
{
    public List<string> Check(TieredScenarioDraft item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered scenario draft.</summary>
public sealed record TieredScenarioDraftValidator(List<ITieredScenarioDraftRule> Rules)
{
    public List<string> Validate(TieredScenarioDraft item)
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

    public void RequireValid(TieredScenarioDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredScenarioDraft> items)
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

/// <summary>Functions of tiered scenario draft rules.</summary>
public static class TieredScenarioDraftRules
{
    public static TieredScenarioDraftValidator DefaultTieredScenarioDraftValidator()
    {
        return new TieredScenarioDraftValidator(
            new List<ITieredScenarioDraftRule>
            {
                new TieredScenarioDraftLabelLengthRule(16),
                new TieredScenarioDraftSequenceRangeRule(32, 64),
                new TieredScenarioDraftCodePrefixRule("REF"),
            });
    }
}
