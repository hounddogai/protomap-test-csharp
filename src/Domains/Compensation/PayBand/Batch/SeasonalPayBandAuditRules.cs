namespace Acme.Workforce.Compensation.PayBand.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a seasonal pay band audit must pass. Check returns its problems, or none.</summary>
public interface ISeasonalPayBandAuditRule
{
    List<string> Check(SeasonalPayBandAudit item);
}

public sealed record SeasonalPayBandAuditTitleLengthRule(int MaxLength) : ISeasonalPayBandAuditRule
{
    public List<string> Check(SeasonalPayBandAudit item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalPayBandAuditCodePrefixRule(string Prefix) : ISeasonalPayBandAuditRule
{
    public List<string> Check(SeasonalPayBandAudit item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalPayBandAuditThresholdRangeRule(double Min, double Max) : ISeasonalPayBandAuditRule
{
    public List<string> Check(SeasonalPayBandAudit item)
    {
        var valueValue = item.Threshold;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"threshold {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalPayBandAuditAllowedCodeRule(List<string> Allowed) : ISeasonalPayBandAuditRule
{
    public List<string> Check(SeasonalPayBandAudit item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is not allowed" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal pay band audit.</summary>
public sealed record SeasonalPayBandAuditValidator(List<ISeasonalPayBandAuditRule> Rules)
{
    public List<string> Validate(SeasonalPayBandAudit item)
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

    public void RequireValid(SeasonalPayBandAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalPayBandAudit> items)
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

/// <summary>Functions of seasonal pay band audit rules.</summary>
public static class SeasonalPayBandAuditRules
{
    public static SeasonalPayBandAuditValidator DefaultSeasonalPayBandAuditValidator()
    {
        return new SeasonalPayBandAuditValidator(
            new List<ISeasonalPayBandAuditRule>
            {
                new SeasonalPayBandAuditTitleLengthRule(1000),
                new SeasonalPayBandAuditCodePrefixRule("OPS"),
                new SeasonalPayBandAuditThresholdRangeRule(0.6, 1262.8),
                new SeasonalPayBandAuditAllowedCodeRule(new List<string>()),
            });
    }
}
