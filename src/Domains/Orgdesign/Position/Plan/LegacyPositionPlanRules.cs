namespace Acme.Workforce.Orgdesign.Position.Plan;

/// <summary>A rule a legacy position plan must pass. Check returns its problems, or none.</summary>
public interface ILegacyPositionPlanRule
{
    List<string> Check(LegacyPositionPlan item);
}

public sealed record LegacyPositionPlanCapacityRangeRule(int Min, int Max) : ILegacyPositionPlanRule
{
    public List<string> Check(LegacyPositionPlan item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LegacyPositionPlanRequiredCodeRule(string Message) : ILegacyPositionPlanRule
{
    public List<string> Check(LegacyPositionPlan item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyPositionPlanNameLengthRule(int MaxLength) : ILegacyPositionPlanRule
{
    public List<string> Check(LegacyPositionPlan item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyPositionPlanRequiredNameRule(string Message) : ILegacyPositionPlanRule
{
    public List<string> Check(LegacyPositionPlan item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyPositionPlanCountryLengthRule(int MaxLength) : ILegacyPositionPlanRule
{
    public List<string> Check(LegacyPositionPlan item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyPositionPlanCategoryLengthRule(int MaxLength) : ILegacyPositionPlanRule
{
    public List<string> Check(LegacyPositionPlan item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy position plan.</summary>
public sealed record LegacyPositionPlanValidator(List<ILegacyPositionPlanRule> Rules)
{
    public List<string> Validate(LegacyPositionPlan item)
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

    public void RequireValid(LegacyPositionPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyPositionPlan> items)
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

/// <summary>Functions of legacy position plan rules.</summary>
public static class LegacyPositionPlanRules
{
    public static LegacyPositionPlanValidator DefaultLegacyPositionPlanValidator()
    {
        return new LegacyPositionPlanValidator(
            new List<ILegacyPositionPlanRule>
            {
                new LegacyPositionPlanCapacityRangeRule(16, 32),
                new LegacyPositionPlanRequiredCodeRule("legacy position plan needs an entry"),
                new LegacyPositionPlanNameLengthRule(80),
                new LegacyPositionPlanRequiredNameRule("legacy position plan needs an entry"),
                new LegacyPositionPlanCountryLengthRule(120),
                new LegacyPositionPlanCategoryLengthRule(86400),
            });
    }
}
