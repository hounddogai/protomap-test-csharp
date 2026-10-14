namespace Acme.Workforce.Orgdesign.Scenario.ScenarioTemplate;

/// <summary>A rule a capped scenario ledger must pass. Check returns its problems, or none.</summary>
public interface ICappedScenarioLedgerRule
{
    List<string> Check(CappedScenarioLedger item);
}

public sealed record CappedScenarioLedgerRequiredCodeRule(string Message) : ICappedScenarioLedgerRule
{
    public List<string> Check(CappedScenarioLedger item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedScenarioLedgerRequiredTitleRule(string Message) : ICappedScenarioLedgerRule
{
    public List<string> Check(CappedScenarioLedger item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedScenarioLedgerCodeLengthRule(int MaxLength) : ICappedScenarioLedgerRule
{
    public List<string> Check(CappedScenarioLedger item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedScenarioLedgerCountryLengthRule(int MaxLength) : ICappedScenarioLedgerRule
{
    public List<string> Check(CappedScenarioLedger item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped scenario ledger.</summary>
public sealed record CappedScenarioLedgerValidator(List<ICappedScenarioLedgerRule> Rules)
{
    public List<string> Validate(CappedScenarioLedger item)
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

    public void RequireValid(CappedScenarioLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedScenarioLedger> items)
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

/// <summary>Functions of capped scenario ledger rules.</summary>
public static class CappedScenarioLedgerRules
{
    public static CappedScenarioLedgerValidator DefaultCappedScenarioLedgerValidator()
    {
        return new CappedScenarioLedgerValidator(
            new List<ICappedScenarioLedgerRule>
            {
                new CappedScenarioLedgerRequiredCodeRule("capped scenario ledger needs an entry"),
                new CappedScenarioLedgerRequiredTitleRule("capped scenario ledger needs a value"),
                new CappedScenarioLedgerCodeLengthRule(64),
                new CappedScenarioLedgerCountryLengthRule(64),
            });
    }
}
