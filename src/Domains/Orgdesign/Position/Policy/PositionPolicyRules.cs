namespace Acme.Workforce.Orgdesign.Position.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a position policy must pass. Check returns its problems, or none.</summary>
public interface IPositionPolicyRule
{
    List<string> Check(PositionPolicy item);
}

public sealed record PositionPolicyRatioRangeRule(double Min, double Max) : IPositionPolicyRule
{
    public List<string> Check(PositionPolicy item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PositionPolicyCodeLengthRule(int MaxLength) : IPositionPolicyRule
{
    public List<string> Check(PositionPolicy item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PositionPolicyCreatedAtRangeRule(int Min, int Max) : IPositionPolicyRule
{
    public List<string> Check(PositionPolicy item)
    {
        var valueValue = item.CreatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"created at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PositionPolicyLabelLengthRule(int MaxLength) : IPositionPolicyRule
{
    public List<string> Check(PositionPolicy item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PositionPolicyAllowedRegionRule(List<string> Allowed) : IPositionPolicyRule
{
    public List<string> Check(PositionPolicy item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Region)))
        {
            return new List<string> { $"region {item.Region} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record PositionPolicyRequiredRegionRule(string Message) : IPositionPolicyRule
{
    public List<string> Check(PositionPolicy item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a position policy.</summary>
public sealed record PositionPolicyValidator(List<IPositionPolicyRule> Rules)
{
    public List<string> Validate(PositionPolicy item)
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

    public void RequireValid(PositionPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PositionPolicy> items)
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

/// <summary>Functions of position policy rules.</summary>
public static class PositionPolicyRules
{
    public static PositionPolicyValidator DefaultPositionPolicyValidator()
    {
        return new PositionPolicyValidator(
            new List<IPositionPolicyRule>
            {
                new PositionPolicyRatioRangeRule(9.0, 4352.6),
                new PositionPolicyCodeLengthRule(1000),
                new PositionPolicyCreatedAtRangeRule(80, 86400),
                new PositionPolicyLabelLengthRule(1000),
                new PositionPolicyAllowedRegionRule(new List<string> { "default" }),
                new PositionPolicyRequiredRegionRule("position policy needs an entry"),
            });
    }
}
