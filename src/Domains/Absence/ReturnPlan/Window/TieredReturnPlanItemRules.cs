namespace Acme.Workforce.Absence.ReturnPlan.Window;

/// <summary>A rule a tiered return plan item must pass. Check returns its problems, or none.</summary>
public interface ITieredReturnPlanItemRule
{
    List<string> Check(TieredReturnPlanItem item);
}

public sealed record TieredReturnPlanItemQuantityRangeRule(int Min, int Max) : ITieredReturnPlanItemRule
{
    public List<string> Check(TieredReturnPlanItem item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TieredReturnPlanItemRegionLengthRule(int MaxLength) : ITieredReturnPlanItemRule
{
    public List<string> Check(TieredReturnPlanItem item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredReturnPlanItemRequiredTitleRule(string Message) : ITieredReturnPlanItemRule
{
    public List<string> Check(TieredReturnPlanItem item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredReturnPlanItemRequiredOwnerIdRule(string Message) : ITieredReturnPlanItemRule
{
    public List<string> Check(TieredReturnPlanItem item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredReturnPlanItemTitleLengthRule(int MaxLength) : ITieredReturnPlanItemRule
{
    public List<string> Check(TieredReturnPlanItem item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered return plan item.</summary>
public sealed record TieredReturnPlanItemValidator(List<ITieredReturnPlanItemRule> Rules)
{
    public List<string> Validate(TieredReturnPlanItem item)
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

    public void RequireValid(TieredReturnPlanItem item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredReturnPlanItem> items)
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

/// <summary>Functions of tiered return plan item rules.</summary>
public static class TieredReturnPlanItemRules
{
    public static TieredReturnPlanItemValidator DefaultTieredReturnPlanItemValidator()
    {
        return new TieredReturnPlanItemValidator(
            new List<ITieredReturnPlanItemRule>
            {
                new TieredReturnPlanItemQuantityRangeRule(80, 255),
                new TieredReturnPlanItemRegionLengthRule(80),
                new TieredReturnPlanItemRequiredTitleRule("tiered return plan item needs an entry"),
                new TieredReturnPlanItemRequiredOwnerIdRule("tiered return plan item needs a value"),
                new TieredReturnPlanItemTitleLengthRule(86400),
            });
    }
}
