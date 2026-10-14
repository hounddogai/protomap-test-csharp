namespace Acme.Workforce.Workplace.Occupancy.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a occupancy mapping must pass. Check returns its problems, or none.</summary>
public interface IOccupancyMappingRule
{
    List<string> Check(OccupancyMapping item);
}

public sealed record OccupancyMappingCodeLengthRule(int MaxLength) : IOccupancyMappingRule
{
    public List<string> Check(OccupancyMapping item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyMappingMaximumRangeRule(double Min, double Max) : IOccupancyMappingRule
{
    public List<string> Check(OccupancyMapping item)
    {
        var valueValue = item.Maximum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"maximum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"maximum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record OccupancyMappingRequiredNameRule(string Message) : IOccupancyMappingRule
{
    public List<string> Check(OccupancyMapping item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OccupancyMappingAllowedCodeRule(List<string> Allowed) : IOccupancyMappingRule
{
    public List<string> Check(OccupancyMapping item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is expired" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a occupancy mapping.</summary>
public sealed record OccupancyMappingValidator(List<IOccupancyMappingRule> Rules)
{
    public List<string> Validate(OccupancyMapping item)
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

    public void RequireValid(OccupancyMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OccupancyMapping> items)
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

/// <summary>Functions of occupancy mapping rules.</summary>
public static class OccupancyMappingRules
{
    public static OccupancyMappingValidator DefaultOccupancyMappingValidator()
    {
        return new OccupancyMappingValidator(
            new List<IOccupancyMappingRule>
            {
                new OccupancyMappingCodeLengthRule(32),
                new OccupancyMappingMaximumRangeRule(2.7, 4068.7),
                new OccupancyMappingRequiredNameRule("occupancy mapping needs a value"),
                new OccupancyMappingAllowedCodeRule(new List<string> { "tiered" }),
            });
    }
}
