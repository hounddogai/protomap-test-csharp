namespace Acme.Workforce.Talent.Mobility.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a mobility group must pass. Check returns its problems, or none.</summary>
public interface IMobilityGroupRule
{
    List<string> Check(MobilityGroup item);
}

public sealed record MobilityGroupVisibleGuardRule(int Limit) : IMobilityGroupRule
{
    public List<string> Check(MobilityGroup item)
    {
        if (item.Visible && item.Quantity > Limit)
        {
            return new List<string> { $"quantity {item.Quantity} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record MobilityGroupCodePrefixRule(string Prefix) : IMobilityGroupRule
{
    public List<string> Check(MobilityGroup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record MobilityGroupAllowedCodeRule(List<string> Allowed) : IMobilityGroupRule
{
    public List<string> Check(MobilityGroup item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record MobilityGroupRequiredCodeRule(string Message) : IMobilityGroupRule
{
    public List<string> Check(MobilityGroup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MobilityGroupNameLengthRule(int MaxLength) : IMobilityGroupRule
{
    public List<string> Check(MobilityGroup item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MobilityGroupCodeLengthRule(int MaxLength) : IMobilityGroupRule
{
    public List<string> Check(MobilityGroup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a mobility group.</summary>
public sealed record MobilityGroupValidator(List<IMobilityGroupRule> Rules)
{
    public List<string> Validate(MobilityGroup item)
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

    public void RequireValid(MobilityGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MobilityGroup> items)
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

/// <summary>Functions of mobility group rules.</summary>
public static class MobilityGroupRules
{
    public static MobilityGroupValidator DefaultMobilityGroupValidator()
    {
        return new MobilityGroupValidator(
            new List<IMobilityGroupRule>
            {
                new MobilityGroupVisibleGuardRule(3600),
                new MobilityGroupCodePrefixRule("X"),
                new MobilityGroupAllowedCodeRule(new List<string> { "lenient", "primary" }),
                new MobilityGroupRequiredCodeRule("mobility group needs this field"),
                new MobilityGroupNameLengthRule(1000),
                new MobilityGroupCodeLengthRule(120),
            });
    }
}
