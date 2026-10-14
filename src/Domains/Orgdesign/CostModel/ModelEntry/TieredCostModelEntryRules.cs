namespace Acme.Workforce.Orgdesign.CostModel.ModelEntry;

/// <summary>A rule a tiered cost model entry must pass. Check returns its problems, or none.</summary>
public interface ITieredCostModelEntryRule
{
    List<string> Check(TieredCostModelEntry item);
}

public sealed record TieredCostModelEntryCodePrefixRule(string Prefix) : ITieredCostModelEntryRule
{
    public List<string> Check(TieredCostModelEntry item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TieredCostModelEntryBudgetRangeRule(double Min, double Max) : ITieredCostModelEntryRule
{
    public List<string> Check(TieredCostModelEntry item)
    {
        var valueValue = item.Budget;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"budget {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"budget {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TieredCostModelEntryRequiredTitleRule(string Message) : ITieredCostModelEntryRule
{
    public List<string> Check(TieredCostModelEntry item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered cost model entry.</summary>
public sealed record TieredCostModelEntryValidator(List<ITieredCostModelEntryRule> Rules)
{
    public List<string> Validate(TieredCostModelEntry item)
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

    public void RequireValid(TieredCostModelEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredCostModelEntry> items)
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

/// <summary>Functions of tiered cost model entry rules.</summary>
public static class TieredCostModelEntryRules
{
    public static TieredCostModelEntryValidator DefaultTieredCostModelEntryValidator()
    {
        return new TieredCostModelEntryValidator(
            new List<ITieredCostModelEntryRule>
            {
                new TieredCostModelEntryCodePrefixRule("X"),
                new TieredCostModelEntryBudgetRangeRule(3.5, 1582.8),
                new TieredCostModelEntryRequiredTitleRule("tiered cost model entry needs this field"),
            });
    }
}
