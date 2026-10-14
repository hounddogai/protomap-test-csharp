namespace Acme.Workforce.Workplace.Occupancy.OccupancyAgreement;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a occupancy template must pass. Check returns its problems, or none.</summary>
public interface IOccupancyTemplateRule
{
    List<string> Check(OccupancyTemplate item);
}

public sealed record OccupancyTemplateAllowedContactEmailRule(List<string> Allowed) : IOccupancyTemplateRule
{
    public List<string> Check(OccupancyTemplate item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ContactEmail)))
        {
            return new List<string> { $"contact email {item.ContactEmail} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyTemplateCodePrefixRule(string Prefix) : IOccupancyTemplateRule
{
    public List<string> Check(OccupancyTemplate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyTemplateFloorValueRangeRule(double Min, double Max) : IOccupancyTemplateRule
{
    public List<string> Check(OccupancyTemplate item)
    {
        var valueValue = item.FloorValue;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"floor value {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"floor value {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a occupancy template.</summary>
public sealed record OccupancyTemplateValidator(List<IOccupancyTemplateRule> Rules)
{
    public List<string> Validate(OccupancyTemplate item)
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

    public void RequireValid(OccupancyTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OccupancyTemplate> items)
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

/// <summary>Functions of occupancy template rules.</summary>
public static class OccupancyTemplateRules
{
    public static OccupancyTemplateValidator DefaultOccupancyTemplateValidator()
    {
        return new OccupancyTemplateValidator(
            new List<IOccupancyTemplateRule>
            {
                new OccupancyTemplateAllowedContactEmailRule(new List<string> { "tiered" }),
                new OccupancyTemplateCodePrefixRule("X"),
                new OccupancyTemplateFloorValueRangeRule(2.9, 4098.0),
            });
    }
}
