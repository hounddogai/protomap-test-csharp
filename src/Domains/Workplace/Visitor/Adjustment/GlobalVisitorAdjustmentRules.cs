namespace Acme.Workforce.Workplace.Visitor.Adjustment;

/// <summary>A rule a global visitor adjustment must pass. Check returns its problems, or none.</summary>
public interface IGlobalVisitorAdjustmentRule
{
    List<string> Check(GlobalVisitorAdjustment item);
}

public sealed record GlobalVisitorAdjustmentRequiredManagerIdRule(string Message) : IGlobalVisitorAdjustmentRule
{
    public List<string> Check(GlobalVisitorAdjustment item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalVisitorAdjustmentQuantityRangeRule(int Min, int Max) : IGlobalVisitorAdjustmentRule
{
    public List<string> Check(GlobalVisitorAdjustment item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalVisitorAdjustmentLabelLengthRule(int MaxLength) : IGlobalVisitorAdjustmentRule
{
    public List<string> Check(GlobalVisitorAdjustment item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global visitor adjustment.</summary>
public sealed record GlobalVisitorAdjustmentValidator(List<IGlobalVisitorAdjustmentRule> Rules)
{
    public List<string> Validate(GlobalVisitorAdjustment item)
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

    public void RequireValid(GlobalVisitorAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalVisitorAdjustment> items)
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

/// <summary>Functions of global visitor adjustment rules.</summary>
public static class GlobalVisitorAdjustmentRules
{
    public static GlobalVisitorAdjustmentValidator DefaultGlobalVisitorAdjustmentValidator()
    {
        return new GlobalVisitorAdjustmentValidator(
            new List<IGlobalVisitorAdjustmentRule>
            {
                new GlobalVisitorAdjustmentRequiredManagerIdRule("global visitor adjustment needs an entry"),
                new GlobalVisitorAdjustmentQuantityRangeRule(255, 1000),
                new GlobalVisitorAdjustmentLabelLengthRule(120),
            });
    }
}
