namespace Acme.Workforce.Workplace.Occupancy.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a occupancy profile must pass. Check returns its problems, or none.</summary>
public interface IOccupancyProfileRule
{
    List<string> Check(OccupancyProfile item);
}

public sealed record OccupancyProfileAllowedCodeRule(List<string> Allowed) : IOccupancyProfileRule
{
    public List<string> Check(OccupancyProfile item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyProfileCodePrefixRule(string Prefix) : IOccupancyProfileRule
{
    public List<string> Check(OccupancyProfile item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyProfileRequiredCodeRule(string Message) : IOccupancyProfileRule
{
    public List<string> Check(OccupancyProfile item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a occupancy profile.</summary>
public sealed record OccupancyProfileValidator(List<IOccupancyProfileRule> Rules)
{
    public List<string> Validate(OccupancyProfile item)
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

    public void RequireValid(OccupancyProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OccupancyProfile> items)
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

/// <summary>Functions of occupancy profile rules.</summary>
public static class OccupancyProfileRules
{
    public static OccupancyProfileValidator DefaultOccupancyProfileValidator()
    {
        return new OccupancyProfileValidator(
            new List<IOccupancyProfileRule>
            {
                new OccupancyProfileAllowedCodeRule(new List<string> { "scheduled", "fallback" }),
                new OccupancyProfileCodePrefixRule("OPS"),
                new OccupancyProfileRequiredCodeRule("occupancy profile needs an entry"),
            });
    }
}
