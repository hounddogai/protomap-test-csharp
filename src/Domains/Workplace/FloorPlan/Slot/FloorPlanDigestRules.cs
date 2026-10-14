namespace Acme.Workforce.Workplace.FloorPlan.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a floor plan digest must pass. Check returns its problems, or none.</summary>
public interface IFloorPlanDigestRule
{
    List<string> Check(FloorPlanDigest item);
}

public sealed record FloorPlanDigestRequiredCodeRule(string Message) : IFloorPlanDigestRule
{
    public List<string> Check(FloorPlanDigest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanDigestLabelLengthRule(int MaxLength) : IFloorPlanDigestRule
{
    public List<string> Check(FloorPlanDigest item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanDigestAllowedUnitRule(List<string> Allowed) : IFloorPlanDigestRule
{
    public List<string> Check(FloorPlanDigest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Unit)))
        {
            return new List<string> { $"unit {item.Unit} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanDigestRequiredNotesRule(string Message) : IFloorPlanDigestRule
{
    public List<string> Check(FloorPlanDigest item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanDigestDescriptionLengthRule(int MaxLength) : IFloorPlanDigestRule
{
    public List<string> Check(FloorPlanDigest item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanDigestCodeLengthRule(int MaxLength) : IFloorPlanDigestRule
{
    public List<string> Check(FloorPlanDigest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a floor plan digest.</summary>
public sealed record FloorPlanDigestValidator(List<IFloorPlanDigestRule> Rules)
{
    public List<string> Validate(FloorPlanDigest item)
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

    public void RequireValid(FloorPlanDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FloorPlanDigest> items)
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

/// <summary>Functions of floor plan digest rules.</summary>
public static class FloorPlanDigestRules
{
    public static FloorPlanDigestValidator DefaultFloorPlanDigestValidator()
    {
        return new FloorPlanDigestValidator(
            new List<IFloorPlanDigestRule>
            {
                new FloorPlanDigestRequiredCodeRule("floor plan digest needs a value"),
                new FloorPlanDigestLabelLengthRule(120),
                new FloorPlanDigestAllowedUnitRule(new List<string> { "global", "capped" }),
                new FloorPlanDigestRequiredNotesRule("floor plan digest needs this field"),
                new FloorPlanDigestDescriptionLengthRule(16),
                new FloorPlanDigestCodeLengthRule(64),
            });
    }
}
