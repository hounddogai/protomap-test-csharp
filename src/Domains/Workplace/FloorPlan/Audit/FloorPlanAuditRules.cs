namespace Acme.Workforce.Workplace.FloorPlan.Audit;

/// <summary>A rule a floor plan audit must pass. Check returns its problems, or none.</summary>
public interface IFloorPlanAuditRule
{
    List<string> Check(FloorPlanAudit item);
}

public sealed record FloorPlanAuditCodePrefixRule(string Prefix) : IFloorPlanAuditRule
{
    public List<string> Check(FloorPlanAudit item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanAuditDiscountRangeRule(double Min, double Max) : IFloorPlanAuditRule
{
    public List<string> Check(FloorPlanAudit item)
    {
        var valueValue = item.Discount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"discount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"discount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FloorPlanAuditRequiredTitleRule(string Message) : IFloorPlanAuditRule
{
    public List<string> Check(FloorPlanAudit item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanAuditRequiredCodeRule(string Message) : IFloorPlanAuditRule
{
    public List<string> Check(FloorPlanAudit item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanAuditCurrencyLengthRule(int MaxLength) : IFloorPlanAuditRule
{
    public List<string> Check(FloorPlanAudit item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a floor plan audit.</summary>
public sealed record FloorPlanAuditValidator(List<IFloorPlanAuditRule> Rules)
{
    public List<string> Validate(FloorPlanAudit item)
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

    public void RequireValid(FloorPlanAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FloorPlanAudit> items)
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

/// <summary>Functions of floor plan audit rules.</summary>
public static class FloorPlanAuditRules
{
    public static FloorPlanAuditValidator DefaultFloorPlanAuditValidator()
    {
        return new FloorPlanAuditValidator(
            new List<IFloorPlanAuditRule>
            {
                new FloorPlanAuditCodePrefixRule("X"),
                new FloorPlanAuditDiscountRangeRule(4.8, 3087.4),
                new FloorPlanAuditRequiredTitleRule("floor plan audit needs an entry"),
                new FloorPlanAuditRequiredCodeRule("floor plan audit needs an entry"),
                new FloorPlanAuditCurrencyLengthRule(3600),
            });
    }
}
