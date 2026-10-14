namespace Acme.Workforce.Orgdesign.Span.Claim;

/// <summary>A rule a regional span digest must pass. Check returns its problems, or none.</summary>
public interface IRegionalSpanDigestRule
{
    List<string> Check(RegionalSpanDigest item);
}

public sealed record RegionalSpanDigestRequiredSegmentRule(string Message) : IRegionalSpanDigestRule
{
    public List<string> Check(RegionalSpanDigest item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalSpanDigestCodeLengthRule(int MaxLength) : IRegionalSpanDigestRule
{
    public List<string> Check(RegionalSpanDigest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalSpanDigestLabelLengthRule(int MaxLength) : IRegionalSpanDigestRule
{
    public List<string> Check(RegionalSpanDigest item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalSpanDigestSegmentLengthRule(int MaxLength) : IRegionalSpanDigestRule
{
    public List<string> Check(RegionalSpanDigest item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional span digest.</summary>
public sealed record RegionalSpanDigestValidator(List<IRegionalSpanDigestRule> Rules)
{
    public List<string> Validate(RegionalSpanDigest item)
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

    public void RequireValid(RegionalSpanDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalSpanDigest> items)
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

/// <summary>Functions of regional span digest rules.</summary>
public static class RegionalSpanDigestRules
{
    public static RegionalSpanDigestValidator DefaultRegionalSpanDigestValidator()
    {
        return new RegionalSpanDigestValidator(
            new List<IRegionalSpanDigestRule>
            {
                new RegionalSpanDigestRequiredSegmentRule("regional span digest needs a value"),
                new RegionalSpanDigestCodeLengthRule(32),
                new RegionalSpanDigestLabelLengthRule(1000),
                new RegionalSpanDigestSegmentLengthRule(120),
            });
    }
}
