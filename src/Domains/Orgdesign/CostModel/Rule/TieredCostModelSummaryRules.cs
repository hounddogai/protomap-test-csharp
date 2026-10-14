namespace Acme.Workforce.Orgdesign.CostModel.Rule;

/// <summary>A rule a tiered cost model summary must pass. Check returns its problems, or none.</summary>
public interface ITieredCostModelSummaryRule
{
    List<string> Check(TieredCostModelSummary item);
}

public sealed record TieredCostModelSummaryPinnedGuardRule(int Limit) : ITieredCostModelSummaryRule
{
    public List<string> Check(TieredCostModelSummary item)
    {
        if (item.Pinned && item.UpdatedAt > Limit)
        {
            return new List<string> { $"updated at {item.UpdatedAt} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record TieredCostModelSummaryTitleLengthRule(int MaxLength) : ITieredCostModelSummaryRule
{
    public List<string> Check(TieredCostModelSummary item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredCostModelSummaryRequiredDepartmentRule(string Message) : ITieredCostModelSummaryRule
{
    public List<string> Check(TieredCostModelSummary item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered cost model summary.</summary>
public sealed record TieredCostModelSummaryValidator(List<ITieredCostModelSummaryRule> Rules)
{
    public List<string> Validate(TieredCostModelSummary item)
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

    public void RequireValid(TieredCostModelSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredCostModelSummary> items)
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

/// <summary>Functions of tiered cost model summary rules.</summary>
public static class TieredCostModelSummaryRules
{
    public static TieredCostModelSummaryValidator DefaultTieredCostModelSummaryValidator()
    {
        return new TieredCostModelSummaryValidator(
            new List<ITieredCostModelSummaryRule>
            {
                new TieredCostModelSummaryPinnedGuardRule(32),
                new TieredCostModelSummaryTitleLengthRule(120),
                new TieredCostModelSummaryRequiredDepartmentRule("tiered cost model summary needs a value"),
            });
    }
}
