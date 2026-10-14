namespace Acme.Workforce.Orgdesign.Span.SpanDraft;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a default span claim must pass. Check returns its problems, or none.</summary>
public interface IDefaultSpanClaimRule
{
    List<string> Check(DefaultSpanClaim item);
}

public sealed record DefaultSpanClaimUnitLengthRule(int MaxLength) : IDefaultSpanClaimRule
{
    public List<string> Check(DefaultSpanClaim item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultSpanClaimMinimumRangeRule(double Min, double Max) : IDefaultSpanClaimRule
{
    public List<string> Check(DefaultSpanClaim item)
    {
        var valueValue = item.Minimum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"minimum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultSpanClaimRequiredLabelRule(string Message) : IDefaultSpanClaimRule
{
    public List<string> Check(DefaultSpanClaim item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultSpanClaimRequiredCodeRule(string Message) : IDefaultSpanClaimRule
{
    public List<string> Check(DefaultSpanClaim item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultSpanClaimRequiredUnitRule(string Message) : IDefaultSpanClaimRule
{
    public List<string> Check(DefaultSpanClaim item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultSpanClaimAllowedLabelRule(List<string> Allowed) : IDefaultSpanClaimRule
{
    public List<string> Check(DefaultSpanClaim item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is unknown" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default span claim.</summary>
public sealed record DefaultSpanClaimValidator(List<IDefaultSpanClaimRule> Rules)
{
    public List<string> Validate(DefaultSpanClaim item)
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

    public void RequireValid(DefaultSpanClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultSpanClaim> items)
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

/// <summary>Functions of default span claim rules.</summary>
public static class DefaultSpanClaimRules
{
    public static DefaultSpanClaimValidator DefaultDefaultSpanClaimValidator()
    {
        return new DefaultSpanClaimValidator(
            new List<IDefaultSpanClaimRule>
            {
                new DefaultSpanClaimUnitLengthRule(64),
                new DefaultSpanClaimMinimumRangeRule(3.0, 4612.8),
                new DefaultSpanClaimRequiredLabelRule("default span claim needs a value"),
                new DefaultSpanClaimRequiredCodeRule("default span claim needs this field"),
                new DefaultSpanClaimRequiredUnitRule("default span claim needs this field"),
                new DefaultSpanClaimAllowedLabelRule(new List<string> { "pending", "lenient" }),
            });
    }
}
