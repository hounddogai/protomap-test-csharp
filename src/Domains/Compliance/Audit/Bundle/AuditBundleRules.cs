namespace Acme.Workforce.Compliance.Audit.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a audit bundle must pass. Check returns its problems, or none.</summary>
public interface IAuditBundleRule
{
    List<string> Check(AuditBundle item);
}

public sealed record AuditBundleCodeLengthRule(int MaxLength) : IAuditBundleRule
{
    public List<string> Check(AuditBundle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AuditBundleRequiredCodeRule(string Message) : IAuditBundleRule
{
    public List<string> Check(AuditBundle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AuditBundleRequiredBadgeRule(string Message) : IAuditBundleRule
{
    public List<string> Check(AuditBundle item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AuditBundleDurationMinutesRangeRule(int Min, int Max) : IAuditBundleRule
{
    public List<string> Check(AuditBundle item)
    {
        var valueValue = item.DurationMinutes;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"duration minutes {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AuditBundleAllowedCodeRule(List<string> Allowed) : IAuditBundleRule
{
    public List<string> Check(AuditBundle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is missing" };
        }
        return new List<string>();
    }
}

public sealed record AuditBundleRatioRangeRule(double Min, double Max) : IAuditBundleRule
{
    public List<string> Check(AuditBundle item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a audit bundle.</summary>
public sealed record AuditBundleValidator(List<IAuditBundleRule> Rules)
{
    public List<string> Validate(AuditBundle item)
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

    public void RequireValid(AuditBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AuditBundle> items)
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

/// <summary>Functions of audit bundle rules.</summary>
public static class AuditBundleRules
{
    public static AuditBundleValidator DefaultAuditBundleValidator()
    {
        return new AuditBundleValidator(
            new List<IAuditBundleRule>
            {
                new AuditBundleCodeLengthRule(32),
                new AuditBundleRequiredCodeRule("audit bundle needs this field"),
                new AuditBundleRequiredBadgeRule("audit bundle needs this field"),
                new AuditBundleDurationMinutesRangeRule(255, 3600),
                new AuditBundleAllowedCodeRule(new List<string> { "strict", "global" }),
                new AuditBundleRatioRangeRule(2.1, 3175.4),
            });
    }
}
