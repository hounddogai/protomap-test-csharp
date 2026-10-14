namespace Acme.Workforce.Orgdesign.Scenario.ScenarioAudit;

/// <summary>A rule a primary scenario tier must pass. Check returns its problems, or none.</summary>
public interface IPrimaryScenarioTierRule
{
    List<string> Check(PrimaryScenarioTier item);
}

public sealed record PrimaryScenarioTierTitleLengthRule(int MaxLength) : IPrimaryScenarioTierRule
{
    public List<string> Check(PrimaryScenarioTier item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryScenarioTierRequiredCodeRule(string Message) : IPrimaryScenarioTierRule
{
    public List<string> Check(PrimaryScenarioTier item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryScenarioTierWindowDaysRangeRule(int Min, int Max) : IPrimaryScenarioTierRule
{
    public List<string> Check(PrimaryScenarioTier item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryScenarioTierCostCenterLengthRule(int MaxLength) : IPrimaryScenarioTierRule
{
    public List<string> Check(PrimaryScenarioTier item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryScenarioTierAmountRangeRule(double Min, double Max) : IPrimaryScenarioTierRule
{
    public List<string> Check(PrimaryScenarioTier item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a primary scenario tier.</summary>
public sealed record PrimaryScenarioTierValidator(List<IPrimaryScenarioTierRule> Rules)
{
    public List<string> Validate(PrimaryScenarioTier item)
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

    public void RequireValid(PrimaryScenarioTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryScenarioTier> items)
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

/// <summary>Functions of primary scenario tier rules.</summary>
public static class PrimaryScenarioTierRules
{
    public static PrimaryScenarioTierValidator DefaultPrimaryScenarioTierValidator()
    {
        return new PrimaryScenarioTierValidator(
            new List<IPrimaryScenarioTierRule>
            {
                new PrimaryScenarioTierTitleLengthRule(255),
                new PrimaryScenarioTierRequiredCodeRule("primary scenario tier needs this field"),
                new PrimaryScenarioTierWindowDaysRangeRule(120, 1000),
                new PrimaryScenarioTierCostCenterLengthRule(64),
                new PrimaryScenarioTierAmountRangeRule(5.9, 1647.9),
            });
    }
}
