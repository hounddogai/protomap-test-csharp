namespace Acme.Workforce.Contractors.Vendor.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a default vendor segment must pass. Check returns its problems, or none.</summary>
public interface IDefaultVendorSegmentRule
{
    List<string> Check(DefaultVendorSegment item);
}

public sealed record DefaultVendorSegmentScoreRangeRule(double Min, double Max) : IDefaultVendorSegmentRule
{
    public List<string> Check(DefaultVendorSegment item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultVendorSegmentAllowedPhoneRule(List<string> Allowed) : IDefaultVendorSegmentRule
{
    public List<string> Check(DefaultVendorSegment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Phone)))
        {
            return new List<string> { $"phone {item.Phone} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record DefaultVendorSegmentCodePrefixRule(string Prefix) : IDefaultVendorSegmentRule
{
    public List<string> Check(DefaultVendorSegment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultVendorSegmentLimitRangeRule(int Min, int Max) : IDefaultVendorSegmentRule
{
    public List<string> Check(DefaultVendorSegment item)
    {
        var valueValue = item.Limit;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"limit {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"limit {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a default vendor segment.</summary>
public sealed record DefaultVendorSegmentValidator(List<IDefaultVendorSegmentRule> Rules)
{
    public List<string> Validate(DefaultVendorSegment item)
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

    public void RequireValid(DefaultVendorSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultVendorSegment> items)
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

/// <summary>Functions of default vendor segment rules.</summary>
public static class DefaultVendorSegmentRules
{
    public static DefaultVendorSegmentValidator DefaultDefaultVendorSegmentValidator()
    {
        return new DefaultVendorSegmentValidator(
            new List<IDefaultVendorSegmentRule>
            {
                new DefaultVendorSegmentScoreRangeRule(0.9, 4538.0),
                new DefaultVendorSegmentAllowedPhoneRule(new List<string> { "manual" }),
                new DefaultVendorSegmentCodePrefixRule("REF"),
                new DefaultVendorSegmentLimitRangeRule(64, 86400),
            });
    }
}
