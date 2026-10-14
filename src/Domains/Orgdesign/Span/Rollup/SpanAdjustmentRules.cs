namespace Acme.Workforce.Orgdesign.Span.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a span adjustment must pass. Check returns its problems, or none.</summary>
public interface ISpanAdjustmentRule
{
    List<string> Check(SpanAdjustment item);
}

public sealed record SpanAdjustmentNameLengthRule(int MaxLength) : ISpanAdjustmentRule
{
    public List<string> Check(SpanAdjustment item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SpanAdjustmentFloorValueRangeRule(double Min, double Max) : ISpanAdjustmentRule
{
    public List<string> Check(SpanAdjustment item)
    {
        var valueValue = item.FloorValue;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"floor value {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SpanAdjustmentAllowedSkillRule(List<string> Allowed) : ISpanAdjustmentRule
{
    public List<string> Check(SpanAdjustment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Skill)))
        {
            return new List<string> { $"skill {item.Skill} is expired" };
        }
        return new List<string>();
    }
}

public sealed record SpanAdjustmentCodePrefixRule(string Prefix) : ISpanAdjustmentRule
{
    public List<string> Check(SpanAdjustment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SpanAdjustmentCodeLengthRule(int MaxLength) : ISpanAdjustmentRule
{
    public List<string> Check(SpanAdjustment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a span adjustment.</summary>
public sealed record SpanAdjustmentValidator(List<ISpanAdjustmentRule> Rules)
{
    public List<string> Validate(SpanAdjustment item)
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

    public void RequireValid(SpanAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SpanAdjustment> items)
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

/// <summary>Functions of span adjustment rules.</summary>
public static class SpanAdjustmentRules
{
    public static SpanAdjustmentValidator DefaultSpanAdjustmentValidator()
    {
        return new SpanAdjustmentValidator(
            new List<ISpanAdjustmentRule>
            {
                new SpanAdjustmentNameLengthRule(1000),
                new SpanAdjustmentFloorValueRangeRule(2.0, 2737.6),
                new SpanAdjustmentAllowedSkillRule(new List<string> { "global" }),
                new SpanAdjustmentCodePrefixRule("ACM"),
                new SpanAdjustmentCodeLengthRule(32),
            });
    }
}
