namespace Acme.Workforce.Payrollops.RetroPay.Quota;

/// <summary>A rule a retro pay adjustment must pass. Check returns its problems, or none.</summary>
public interface IRetroPayAdjustmentRule
{
    List<string> Check(RetroPayAdjustment item);
}

public sealed record RetroPayAdjustmentCodePrefixRule(string Prefix) : IRetroPayAdjustmentRule
{
    public List<string> Check(RetroPayAdjustment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayAdjustmentRequiredLabelRule(string Message) : IRetroPayAdjustmentRule
{
    public List<string> Check(RetroPayAdjustment item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RetroPayAdjustmentRequiredCodeRule(string Message) : IRetroPayAdjustmentRule
{
    public List<string> Check(RetroPayAdjustment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RetroPayAdjustmentCostCenterLengthRule(int MaxLength) : IRetroPayAdjustmentRule
{
    public List<string> Check(RetroPayAdjustment item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a retro pay adjustment.</summary>
public sealed record RetroPayAdjustmentValidator(List<IRetroPayAdjustmentRule> Rules)
{
    public List<string> Validate(RetroPayAdjustment item)
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

    public void RequireValid(RetroPayAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RetroPayAdjustment> items)
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

/// <summary>Functions of retro pay adjustment rules.</summary>
public static class RetroPayAdjustmentRules
{
    public static RetroPayAdjustmentValidator DefaultRetroPayAdjustmentValidator()
    {
        return new RetroPayAdjustmentValidator(
            new List<IRetroPayAdjustmentRule>
            {
                new RetroPayAdjustmentCodePrefixRule("X"),
                new RetroPayAdjustmentRequiredLabelRule("retro pay adjustment needs a value"),
                new RetroPayAdjustmentRequiredCodeRule("retro pay adjustment needs a value"),
                new RetroPayAdjustmentCostCenterLengthRule(120),
            });
    }
}
