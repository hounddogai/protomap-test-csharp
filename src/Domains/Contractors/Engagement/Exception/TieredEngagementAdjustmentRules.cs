namespace Acme.Workforce.Contractors.Engagement.Exception;

/// <summary>A rule a tiered engagement adjustment must pass. Check returns its problems, or none.</summary>
public interface ITieredEngagementAdjustmentRule
{
    List<string> Check(TieredEngagementAdjustment item);
}

public sealed record TieredEngagementAdjustmentQuantityRangeRule(int Min, int Max) : ITieredEngagementAdjustmentRule
{
    public List<string> Check(TieredEngagementAdjustment item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TieredEngagementAdjustmentCodeLengthRule(int MaxLength) : ITieredEngagementAdjustmentRule
{
    public List<string> Check(TieredEngagementAdjustment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredEngagementAdjustmentLabelLengthRule(int MaxLength) : ITieredEngagementAdjustmentRule
{
    public List<string> Check(TieredEngagementAdjustment item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredEngagementAdjustmentRequiredLabelRule(string Message) : ITieredEngagementAdjustmentRule
{
    public List<string> Check(TieredEngagementAdjustment item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered engagement adjustment.</summary>
public sealed record TieredEngagementAdjustmentValidator(List<ITieredEngagementAdjustmentRule> Rules)
{
    public List<string> Validate(TieredEngagementAdjustment item)
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

    public void RequireValid(TieredEngagementAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredEngagementAdjustment> items)
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

/// <summary>Functions of tiered engagement adjustment rules.</summary>
public static class TieredEngagementAdjustmentRules
{
    public static TieredEngagementAdjustmentValidator DefaultTieredEngagementAdjustmentValidator()
    {
        return new TieredEngagementAdjustmentValidator(
            new List<ITieredEngagementAdjustmentRule>
            {
                new TieredEngagementAdjustmentQuantityRangeRule(16, 86400),
                new TieredEngagementAdjustmentCodeLengthRule(16),
                new TieredEngagementAdjustmentLabelLengthRule(64),
                new TieredEngagementAdjustmentRequiredLabelRule("tiered engagement adjustment needs this field"),
            });
    }
}
