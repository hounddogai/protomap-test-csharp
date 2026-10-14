namespace Acme.Workforce.Orgdesign.Reorg.ReorgProfile;

/// <summary>A rule a reorg adjustment must pass. Check returns its problems, or none.</summary>
public interface IReorgAdjustmentRule
{
    List<string> Check(ReorgAdjustment item);
}

public sealed record ReorgAdjustmentRequiredLabelRule(string Message) : IReorgAdjustmentRule
{
    public List<string> Check(ReorgAdjustment item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReorgAdjustmentRequiredCodeRule(string Message) : IReorgAdjustmentRule
{
    public List<string> Check(ReorgAdjustment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReorgAdjustmentBadgeLengthRule(int MaxLength) : IReorgAdjustmentRule
{
    public List<string> Check(ReorgAdjustment item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ReorgAdjustmentRequiredBadgeRule(string Message) : IReorgAdjustmentRule
{
    public List<string> Check(ReorgAdjustment item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReorgAdjustmentLabelLengthRule(int MaxLength) : IReorgAdjustmentRule
{
    public List<string> Check(ReorgAdjustment item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ReorgAdjustmentAmountRangeRule(double Min, double Max) : IReorgAdjustmentRule
{
    public List<string> Check(ReorgAdjustment item)
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

/// <summary>Runs every rule of a reorg adjustment.</summary>
public sealed record ReorgAdjustmentValidator(List<IReorgAdjustmentRule> Rules)
{
    public List<string> Validate(ReorgAdjustment item)
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

    public void RequireValid(ReorgAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReorgAdjustment> items)
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

/// <summary>Functions of reorg adjustment rules.</summary>
public static class ReorgAdjustmentRules
{
    public static ReorgAdjustmentValidator DefaultReorgAdjustmentValidator()
    {
        return new ReorgAdjustmentValidator(
            new List<IReorgAdjustmentRule>
            {
                new ReorgAdjustmentRequiredLabelRule("reorg adjustment needs a value"),
                new ReorgAdjustmentRequiredCodeRule("reorg adjustment needs this field"),
                new ReorgAdjustmentBadgeLengthRule(32),
                new ReorgAdjustmentRequiredBadgeRule("reorg adjustment needs an entry"),
                new ReorgAdjustmentLabelLengthRule(86400),
                new ReorgAdjustmentAmountRangeRule(4.8, 3428.4),
            });
    }
}
