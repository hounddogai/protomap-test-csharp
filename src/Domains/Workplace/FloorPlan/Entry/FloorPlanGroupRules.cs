namespace Acme.Workforce.Workplace.FloorPlan.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a floor plan group must pass. Check returns its problems, or none.</summary>
public interface IFloorPlanGroupRule
{
    List<string> Check(FloorPlanGroup item);
}

public sealed record FloorPlanGroupCodeLengthRule(int MaxLength) : IFloorPlanGroupRule
{
    public List<string> Check(FloorPlanGroup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanGroupLabelLengthRule(int MaxLength) : IFloorPlanGroupRule
{
    public List<string> Check(FloorPlanGroup item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanGroupAllowedCountryRule(List<string> Allowed) : IFloorPlanGroupRule
{
    public List<string> Check(FloorPlanGroup item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Country)))
        {
            return new List<string> { $"country {item.Country} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanGroupRequiredCountryRule(string Message) : IFloorPlanGroupRule
{
    public List<string> Check(FloorPlanGroup item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanGroupRequiredCodeRule(string Message) : IFloorPlanGroupRule
{
    public List<string> Check(FloorPlanGroup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanGroupReviewerLengthRule(int MaxLength) : IFloorPlanGroupRule
{
    public List<string> Check(FloorPlanGroup item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a floor plan group.</summary>
public sealed record FloorPlanGroupValidator(List<IFloorPlanGroupRule> Rules)
{
    public List<string> Validate(FloorPlanGroup item)
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

    public void RequireValid(FloorPlanGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FloorPlanGroup> items)
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

/// <summary>Functions of floor plan group rules.</summary>
public static class FloorPlanGroupRules
{
    public static FloorPlanGroupValidator DefaultFloorPlanGroupValidator()
    {
        return new FloorPlanGroupValidator(
            new List<IFloorPlanGroupRule>
            {
                new FloorPlanGroupCodeLengthRule(3600),
                new FloorPlanGroupLabelLengthRule(64),
                new FloorPlanGroupAllowedCountryRule(new List<string> { "pending", "manual" }),
                new FloorPlanGroupRequiredCountryRule("floor plan group needs a value"),
                new FloorPlanGroupRequiredCodeRule("floor plan group needs this field"),
                new FloorPlanGroupReviewerLengthRule(1000),
            });
    }
}
