namespace Acme.Workforce.Workplace.Occupancy.OccupancyPlan;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a occupancy assignment must pass. Check returns its problems, or none.</summary>
public interface IOccupancyAssignmentRule
{
    List<string> Check(OccupancyAssignment item);
}

public sealed record OccupancyAssignmentUnitLengthRule(int MaxLength) : IOccupancyAssignmentRule
{
    public List<string> Check(OccupancyAssignment item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyAssignmentAllowedNotesRule(List<string> Allowed) : IOccupancyAssignmentRule
{
    public List<string> Check(OccupancyAssignment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Notes)))
        {
            return new List<string> { $"notes {item.Notes} is too long" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyAssignmentRequiredUnitRule(string Message) : IOccupancyAssignmentRule
{
    public List<string> Check(OccupancyAssignment item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OccupancyAssignmentCodePrefixRule(string Prefix) : IOccupancyAssignmentRule
{
    public List<string> Check(OccupancyAssignment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyAssignmentRequiredNameRule(string Message) : IOccupancyAssignmentRule
{
    public List<string> Check(OccupancyAssignment item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a occupancy assignment.</summary>
public sealed record OccupancyAssignmentValidator(List<IOccupancyAssignmentRule> Rules)
{
    public List<string> Validate(OccupancyAssignment item)
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

    public void RequireValid(OccupancyAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OccupancyAssignment> items)
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

/// <summary>Functions of occupancy assignment rules.</summary>
public static class OccupancyAssignmentRules
{
    public static OccupancyAssignmentValidator DefaultOccupancyAssignmentValidator()
    {
        return new OccupancyAssignmentValidator(
            new List<IOccupancyAssignmentRule>
            {
                new OccupancyAssignmentUnitLengthRule(3600),
                new OccupancyAssignmentAllowedNotesRule(new List<string> { "capped", "strict", "pending" }),
                new OccupancyAssignmentRequiredUnitRule("occupancy assignment needs a value"),
                new OccupancyAssignmentCodePrefixRule("REF"),
                new OccupancyAssignmentRequiredNameRule("occupancy assignment needs a value"),
            });
    }
}
