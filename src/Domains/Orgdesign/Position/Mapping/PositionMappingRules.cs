namespace Acme.Workforce.Orgdesign.Position.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a position mapping must pass. Check returns its problems, or none.</summary>
public interface IPositionMappingRule
{
    List<string> Check(PositionMapping item);
}

public sealed record PositionMappingAllowedCodeRule(List<string> Allowed) : IPositionMappingRule
{
    public List<string> Check(PositionMapping item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record PositionMappingRequiredTitleRule(string Message) : IPositionMappingRule
{
    public List<string> Check(PositionMapping item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PositionMappingCodePrefixRule(string Prefix) : IPositionMappingRule
{
    public List<string> Check(PositionMapping item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PositionMappingCapacityRangeRule(int Min, int Max) : IPositionMappingRule
{
    public List<string> Check(PositionMapping item)
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

/// <summary>Runs every rule of a position mapping.</summary>
public sealed record PositionMappingValidator(List<IPositionMappingRule> Rules)
{
    public List<string> Validate(PositionMapping item)
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

    public void RequireValid(PositionMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PositionMapping> items)
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

/// <summary>Functions of position mapping rules.</summary>
public static class PositionMappingRules
{
    public static PositionMappingValidator DefaultPositionMappingValidator()
    {
        return new PositionMappingValidator(
            new List<IPositionMappingRule>
            {
                new PositionMappingAllowedCodeRule(new List<string> { "rolling", "adaptive", "adaptive" }),
                new PositionMappingRequiredTitleRule("position mapping needs an entry"),
                new PositionMappingCodePrefixRule("X"),
                new PositionMappingCapacityRangeRule(32, 64),
            });
    }
}
