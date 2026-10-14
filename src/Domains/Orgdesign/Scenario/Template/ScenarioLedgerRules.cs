namespace Acme.Workforce.Orgdesign.Scenario.Template;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scenario ledger must pass. Check returns its problems, or none.</summary>
public interface IScenarioLedgerRule
{
    List<string> Check(ScenarioLedger item);
}

public sealed record ScenarioLedgerRequiredBadgeRule(string Message) : IScenarioLedgerRule
{
    public List<string> Check(ScenarioLedger item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScenarioLedgerThresholdRangeRule(double Min, double Max) : IScenarioLedgerRule
{
    public List<string> Check(ScenarioLedger item)
    {
        var valueValue = item.Threshold;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"threshold {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"threshold {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScenarioLedgerAllowedTitleRule(List<string> Allowed) : IScenarioLedgerRule
{
    public List<string> Check(ScenarioLedger item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is missing" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scenario ledger.</summary>
public sealed record ScenarioLedgerValidator(List<IScenarioLedgerRule> Rules)
{
    public List<string> Validate(ScenarioLedger item)
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

    public void RequireValid(ScenarioLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScenarioLedger> items)
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

/// <summary>Functions of scenario ledger rules.</summary>
public static class ScenarioLedgerRules
{
    public static ScenarioLedgerValidator DefaultScenarioLedgerValidator()
    {
        return new ScenarioLedgerValidator(
            new List<IScenarioLedgerRule>
            {
                new ScenarioLedgerRequiredBadgeRule("scenario ledger needs a value"),
                new ScenarioLedgerThresholdRangeRule(3.3, 1055.6),
                new ScenarioLedgerAllowedTitleRule(new List<string>()),
            });
    }
}
