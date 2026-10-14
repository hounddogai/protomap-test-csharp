namespace Acme.Workforce.Talent.Mobility.Window;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a mobility window must pass. Check returns its problems, or none.</summary>
public interface IMobilityWindowRule
{
    List<string> Check(MobilityWindow item);
}

public sealed record MobilityWindowCodePrefixRule(string Prefix) : IMobilityWindowRule
{
    public List<string> Check(MobilityWindow item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record MobilityWindowCodeLengthRule(int MaxLength) : IMobilityWindowRule
{
    public List<string> Check(MobilityWindow item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MobilityWindowAllowedSegmentRule(List<string> Allowed) : IMobilityWindowRule
{
    public List<string> Check(MobilityWindow item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Segment)))
        {
            return new List<string> { $"segment {item.Segment} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record MobilityWindowRequiredSegmentRule(string Message) : IMobilityWindowRule
{
    public List<string> Check(MobilityWindow item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MobilityWindowRequiredCodeRule(string Message) : IMobilityWindowRule
{
    public List<string> Check(MobilityWindow item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MobilityWindowSegmentLengthRule(int MaxLength) : IMobilityWindowRule
{
    public List<string> Check(MobilityWindow item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a mobility window.</summary>
public sealed record MobilityWindowValidator(List<IMobilityWindowRule> Rules)
{
    public List<string> Validate(MobilityWindow item)
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

    public void RequireValid(MobilityWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MobilityWindow> items)
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

/// <summary>Functions of mobility window rules.</summary>
public static class MobilityWindowRules
{
    public static MobilityWindowValidator DefaultMobilityWindowValidator()
    {
        return new MobilityWindowValidator(
            new List<IMobilityWindowRule>
            {
                new MobilityWindowCodePrefixRule("OPS"),
                new MobilityWindowCodeLengthRule(86400),
                new MobilityWindowAllowedSegmentRule(new List<string> { "global", "rolling", "regional" }),
                new MobilityWindowRequiredSegmentRule("mobility window needs a value"),
                new MobilityWindowRequiredCodeRule("mobility window needs this field"),
                new MobilityWindowSegmentLengthRule(16),
            });
    }
}
