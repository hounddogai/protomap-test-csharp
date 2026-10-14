namespace Acme.Workforce.Compliance.PolicyAck.AckGroup;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a strict policy ack template must pass. Check returns its problems, or none.</summary>
public interface IStrictPolicyAckTemplateRule
{
    List<string> Check(StrictPolicyAckTemplate item);
}

public sealed record StrictPolicyAckTemplateAllowedUnitRule(List<string> Allowed) : IStrictPolicyAckTemplateRule
{
    public List<string> Check(StrictPolicyAckTemplate item)
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

public sealed record StrictPolicyAckTemplateVisibleGuardRule(double Limit) : IStrictPolicyAckTemplateRule
{
    public List<string> Check(StrictPolicyAckTemplate item)
    {
        if (item.Visible && item.Rating > Limit)
        {
            return new List<string> { $"rating {item.Rating} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record StrictPolicyAckTemplateCodeLengthRule(int MaxLength) : IStrictPolicyAckTemplateRule
{
    public List<string> Check(StrictPolicyAckTemplate item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictPolicyAckTemplateRatingRangeRule(double Min, double Max) : IStrictPolicyAckTemplateRule
{
    public List<string> Check(StrictPolicyAckTemplate item)
    {
        var valueValue = item.Rating;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rating {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StrictPolicyAckTemplateCodePrefixRule(string Prefix) : IStrictPolicyAckTemplateRule
{
    public List<string> Check(StrictPolicyAckTemplate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict policy ack template.</summary>
public sealed record StrictPolicyAckTemplateValidator(List<IStrictPolicyAckTemplateRule> Rules)
{
    public List<string> Validate(StrictPolicyAckTemplate item)
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

    public void RequireValid(StrictPolicyAckTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictPolicyAckTemplate> items)
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

/// <summary>Functions of strict policy ack template rules.</summary>
public static class StrictPolicyAckTemplateRules
{
    public static StrictPolicyAckTemplateValidator DefaultStrictPolicyAckTemplateValidator()
    {
        return new StrictPolicyAckTemplateValidator(
            new List<IStrictPolicyAckTemplateRule>
            {
                new StrictPolicyAckTemplateAllowedUnitRule(new List<string>()),
                new StrictPolicyAckTemplateVisibleGuardRule(0.3),
                new StrictPolicyAckTemplateCodeLengthRule(3600),
                new StrictPolicyAckTemplateRatingRangeRule(2.9, 3499.6),
                new StrictPolicyAckTemplateCodePrefixRule("OPS"),
            });
    }
}
