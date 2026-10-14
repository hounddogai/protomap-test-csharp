namespace Acme.Workforce.Assets.Assignment.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a legacy assignment baseline must pass. Check returns its problems, or none.</summary>
public interface ILegacyAssignmentBaselineRule
{
    List<string> Check(LegacyAssignmentBaseline item);
}

public sealed record LegacyAssignmentBaselineCodePrefixRule(string Prefix) : ILegacyAssignmentBaselineRule
{
    public List<string> Check(LegacyAssignmentBaseline item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyAssignmentBaselineDurationMinutesRangeRule(int Min, int Max) : ILegacyAssignmentBaselineRule
{
    public List<string> Check(LegacyAssignmentBaseline item)
    {
        var valueValue = item.DurationMinutes;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"duration minutes {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"duration minutes {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LegacyAssignmentBaselineAllowedExternalRefRule(List<string> Allowed) : ILegacyAssignmentBaselineRule
{
    public List<string> Check(LegacyAssignmentBaseline item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ExternalRef)))
        {
            return new List<string> { $"external ref {item.ExternalRef} is expired" };
        }
        return new List<string>();
    }
}

public sealed record LegacyAssignmentBaselineRequiredSummaryRule(string Message) : ILegacyAssignmentBaselineRule
{
    public List<string> Check(LegacyAssignmentBaseline item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyAssignmentBaselineRequiredCodeRule(string Message) : ILegacyAssignmentBaselineRule
{
    public List<string> Check(LegacyAssignmentBaseline item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyAssignmentBaselineFloorValueRangeRule(double Min, double Max) : ILegacyAssignmentBaselineRule
{
    public List<string> Check(LegacyAssignmentBaseline item)
    {
        var valueValue = item.FloorValue;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"floor value {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy assignment baseline.</summary>
public sealed record LegacyAssignmentBaselineValidator(List<ILegacyAssignmentBaselineRule> Rules)
{
    public List<string> Validate(LegacyAssignmentBaseline item)
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

    public void RequireValid(LegacyAssignmentBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyAssignmentBaseline> items)
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

/// <summary>Functions of legacy assignment baseline rules.</summary>
public static class LegacyAssignmentBaselineRules
{
    public static LegacyAssignmentBaselineValidator DefaultLegacyAssignmentBaselineValidator()
    {
        return new LegacyAssignmentBaselineValidator(
            new List<ILegacyAssignmentBaselineRule>
            {
                new LegacyAssignmentBaselineCodePrefixRule("Q"),
                new LegacyAssignmentBaselineDurationMinutesRangeRule(64, 120),
                new LegacyAssignmentBaselineAllowedExternalRefRule(new List<string> { "capped" }),
                new LegacyAssignmentBaselineRequiredSummaryRule("legacy assignment baseline needs this field"),
                new LegacyAssignmentBaselineRequiredCodeRule("legacy assignment baseline needs an entry"),
                new LegacyAssignmentBaselineFloorValueRangeRule(2.9, 3812.2),
            });
    }
}
