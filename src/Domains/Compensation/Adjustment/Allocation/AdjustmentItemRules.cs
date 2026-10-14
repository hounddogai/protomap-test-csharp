namespace Acme.Workforce.Compensation.Adjustment.Allocation;

/// <summary>A rule a adjustment item must pass. Check returns its problems, or none.</summary>
public interface IAdjustmentItemRule
{
    List<string> Check(AdjustmentItem item);
}

public sealed record AdjustmentItemRequiredNameRule(string Message) : IAdjustmentItemRule
{
    public List<string> Check(AdjustmentItem item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentItemRequiredRegionRule(string Message) : IAdjustmentItemRule
{
    public List<string> Check(AdjustmentItem item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentItemRegionLengthRule(int MaxLength) : IAdjustmentItemRule
{
    public List<string> Check(AdjustmentItem item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentItemBudgetRangeRule(double Min, double Max) : IAdjustmentItemRule
{
    public List<string> Check(AdjustmentItem item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentItemCodePrefixRule(string Prefix) : IAdjustmentItemRule
{
    public List<string> Check(AdjustmentItem item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adjustment item.</summary>
public sealed record AdjustmentItemValidator(List<IAdjustmentItemRule> Rules)
{
    public List<string> Validate(AdjustmentItem item)
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

    public void RequireValid(AdjustmentItem item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdjustmentItem> items)
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

/// <summary>Functions of adjustment item rules.</summary>
public static class AdjustmentItemRules
{
    public static AdjustmentItemValidator DefaultAdjustmentItemValidator()
    {
        return new AdjustmentItemValidator(
            new List<IAdjustmentItemRule>
            {
                new AdjustmentItemRequiredNameRule("adjustment item needs a value"),
                new AdjustmentItemRequiredRegionRule("adjustment item needs a value"),
                new AdjustmentItemRegionLengthRule(64),
                new AdjustmentItemBudgetRangeRule(1.2, 3029.1),
                new AdjustmentItemCodePrefixRule("OPS"),
            });
    }
}
