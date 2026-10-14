namespace Acme.Workforce.Assets.LicenseSeat.SeatTemplate;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a license seat policy must pass. Check returns its problems, or none.</summary>
public interface ILicenseSeatPolicyRule
{
    List<string> Check(LicenseSeatPolicy item);
}

public sealed record LicenseSeatPolicyRequiredSegmentRule(string Message) : ILicenseSeatPolicyRule
{
    public List<string> Check(LicenseSeatPolicy item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatPolicyCodePrefixRule(string Prefix) : ILicenseSeatPolicyRule
{
    public List<string> Check(LicenseSeatPolicy item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatPolicyAllowedSegmentRule(List<string> Allowed) : ILicenseSeatPolicyRule
{
    public List<string> Check(LicenseSeatPolicy item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Segment)))
        {
            return new List<string> { $"segment {item.Segment} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatPolicyQuantityRangeRule(int Min, int Max) : ILicenseSeatPolicyRule
{
    public List<string> Check(LicenseSeatPolicy item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LicenseSeatPolicyCodeLengthRule(int MaxLength) : ILicenseSeatPolicyRule
{
    public List<string> Check(LicenseSeatPolicy item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatPolicyStartsAtRangeRule(int Min, int Max) : ILicenseSeatPolicyRule
{
    public List<string> Check(LicenseSeatPolicy item)
    {
        var valueValue = item.StartsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"starts at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"starts at {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a license seat policy.</summary>
public sealed record LicenseSeatPolicyValidator(List<ILicenseSeatPolicyRule> Rules)
{
    public List<string> Validate(LicenseSeatPolicy item)
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

    public void RequireValid(LicenseSeatPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LicenseSeatPolicy> items)
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

/// <summary>Functions of license seat policy rules.</summary>
public static class LicenseSeatPolicyRules
{
    public static LicenseSeatPolicyValidator DefaultLicenseSeatPolicyValidator()
    {
        return new LicenseSeatPolicyValidator(
            new List<ILicenseSeatPolicyRule>
            {
                new LicenseSeatPolicyRequiredSegmentRule("license seat policy needs an entry"),
                new LicenseSeatPolicyCodePrefixRule("REF"),
                new LicenseSeatPolicyAllowedSegmentRule(new List<string> { "weighted", "weighted", "default" }),
                new LicenseSeatPolicyQuantityRangeRule(64, 86400),
                new LicenseSeatPolicyCodeLengthRule(120),
                new LicenseSeatPolicyStartsAtRangeRule(120, 86400),
            });
    }
}
