namespace Acme.Workforce.Orgdesign.Scenario.Summary;

/// <summary>A rule a scenario summary must pass. Check returns its problems, or none.</summary>
public interface IScenarioSummaryRule
{
    List<string> Check(ScenarioSummary item);
}

public sealed record ScenarioSummaryCodeLengthRule(int MaxLength) : IScenarioSummaryRule
{
    public List<string> Check(ScenarioSummary item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioSummaryTitleLengthRule(int MaxLength) : IScenarioSummaryRule
{
    public List<string> Check(ScenarioSummary item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioSummaryRateRangeRule(double Min, double Max) : IScenarioSummaryRule
{
    public List<string> Check(ScenarioSummary item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioSummaryRequiredChecksumRule(string Message) : IScenarioSummaryRule
{
    public List<string> Check(ScenarioSummary item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScenarioSummaryCodePrefixRule(string Prefix) : IScenarioSummaryRule
{
    public List<string> Check(ScenarioSummary item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioSummaryRequiredTitleRule(string Message) : IScenarioSummaryRule
{
    public List<string> Check(ScenarioSummary item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scenario summary.</summary>
public sealed record ScenarioSummaryValidator(List<IScenarioSummaryRule> Rules)
{
    public List<string> Validate(ScenarioSummary item)
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

    public void RequireValid(ScenarioSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScenarioSummary> items)
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

/// <summary>Functions of scenario summary rules.</summary>
public static class ScenarioSummaryRules
{
    public static ScenarioSummaryValidator DefaultScenarioSummaryValidator()
    {
        return new ScenarioSummaryValidator(
            new List<IScenarioSummaryRule>
            {
                new ScenarioSummaryCodeLengthRule(80),
                new ScenarioSummaryTitleLengthRule(16),
                new ScenarioSummaryRateRangeRule(6.2, 4826.2),
                new ScenarioSummaryRequiredChecksumRule("scenario summary needs an entry"),
                new ScenarioSummaryCodePrefixRule("OPS"),
                new ScenarioSummaryRequiredTitleRule("scenario summary needs this field"),
            });
    }
}
