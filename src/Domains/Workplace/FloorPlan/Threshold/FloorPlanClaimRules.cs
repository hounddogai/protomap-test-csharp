namespace Acme.Workforce.Workplace.FloorPlan.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a floor plan claim must pass. Check returns its problems, or none.</summary>
public interface IFloorPlanClaimRule
{
    List<string> Check(FloorPlanClaim item);
}

public sealed record FloorPlanClaimNotesLengthRule(int MaxLength) : IFloorPlanClaimRule
{
    public List<string> Check(FloorPlanClaim item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanClaimRequiredNameRule(string Message) : IFloorPlanClaimRule
{
    public List<string> Check(FloorPlanClaim item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanClaimAllowedNameRule(List<string> Allowed) : IFloorPlanClaimRule
{
    public List<string> Check(FloorPlanClaim item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is too long" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a floor plan claim.</summary>
public sealed record FloorPlanClaimValidator(List<IFloorPlanClaimRule> Rules)
{
    public List<string> Validate(FloorPlanClaim item)
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

    public void RequireValid(FloorPlanClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FloorPlanClaim> items)
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

/// <summary>Functions of floor plan claim rules.</summary>
public static class FloorPlanClaimRules
{
    public static FloorPlanClaimValidator DefaultFloorPlanClaimValidator()
    {
        return new FloorPlanClaimValidator(
            new List<IFloorPlanClaimRule>
            {
                new FloorPlanClaimNotesLengthRule(120),
                new FloorPlanClaimRequiredNameRule("floor plan claim needs this field"),
                new FloorPlanClaimAllowedNameRule(new List<string> { "weighted", "scheduled", "batched" }),
            });
    }
}
