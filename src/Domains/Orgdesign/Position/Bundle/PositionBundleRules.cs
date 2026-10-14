namespace Acme.Workforce.Orgdesign.Position.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a position bundle must pass. Check returns its problems, or none.</summary>
public interface IPositionBundleRule
{
    List<string> Check(PositionBundle item);
}

public sealed record PositionBundleAmountRangeRule(double Min, double Max) : IPositionBundleRule
{
    public List<string> Check(PositionBundle item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PositionBundleRequiredNameRule(string Message) : IPositionBundleRule
{
    public List<string> Check(PositionBundle item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PositionBundleRequiredCodeRule(string Message) : IPositionBundleRule
{
    public List<string> Check(PositionBundle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PositionBundleCodePrefixRule(string Prefix) : IPositionBundleRule
{
    public List<string> Check(PositionBundle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PositionBundleRequiredDepartmentRule(string Message) : IPositionBundleRule
{
    public List<string> Check(PositionBundle item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PositionBundleAllowedUnitRule(List<string> Allowed) : IPositionBundleRule
{
    public List<string> Check(PositionBundle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Unit)))
        {
            return new List<string> { $"unit {item.Unit} is not allowed" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a position bundle.</summary>
public sealed record PositionBundleValidator(List<IPositionBundleRule> Rules)
{
    public List<string> Validate(PositionBundle item)
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

    public void RequireValid(PositionBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PositionBundle> items)
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

/// <summary>Functions of position bundle rules.</summary>
public static class PositionBundleRules
{
    public static PositionBundleValidator DefaultPositionBundleValidator()
    {
        return new PositionBundleValidator(
            new List<IPositionBundleRule>
            {
                new PositionBundleAmountRangeRule(5.3, 4363.9),
                new PositionBundleRequiredNameRule("position bundle needs an entry"),
                new PositionBundleRequiredCodeRule("position bundle needs a value"),
                new PositionBundleCodePrefixRule("X"),
                new PositionBundleRequiredDepartmentRule("position bundle needs this field"),
                new PositionBundleAllowedUnitRule(new List<string> { "regional", "fallback", "global" }),
            });
    }
}
