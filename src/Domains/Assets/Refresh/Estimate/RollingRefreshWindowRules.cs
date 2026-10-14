namespace Acme.Workforce.Assets.Refresh.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rolling refresh window must pass. Check returns its problems, or none.</summary>
public interface IRollingRefreshWindowRule
{
    List<string> Check(RollingRefreshWindow item);
}

public sealed record RollingRefreshWindowRequiredRegionRule(string Message) : IRollingRefreshWindowRule
{
    public List<string> Check(RollingRefreshWindow item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingRefreshWindowCodePrefixRule(string Prefix) : IRollingRefreshWindowRule
{
    public List<string> Check(RollingRefreshWindow item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RollingRefreshWindowPhoneLengthRule(int MaxLength) : IRollingRefreshWindowRule
{
    public List<string> Check(RollingRefreshWindow item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingRefreshWindowAllowedCodeRule(List<string> Allowed) : IRollingRefreshWindowRule
{
    public List<string> Check(RollingRefreshWindow item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is blocked" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling refresh window.</summary>
public sealed record RollingRefreshWindowValidator(List<IRollingRefreshWindowRule> Rules)
{
    public List<string> Validate(RollingRefreshWindow item)
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

    public void RequireValid(RollingRefreshWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingRefreshWindow> items)
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

/// <summary>Functions of rolling refresh window rules.</summary>
public static class RollingRefreshWindowRules
{
    public static RollingRefreshWindowValidator DefaultRollingRefreshWindowValidator()
    {
        return new RollingRefreshWindowValidator(
            new List<IRollingRefreshWindowRule>
            {
                new RollingRefreshWindowRequiredRegionRule("rolling refresh window needs this field"),
                new RollingRefreshWindowCodePrefixRule("X"),
                new RollingRefreshWindowPhoneLengthRule(255),
                new RollingRefreshWindowAllowedCodeRule(new List<string> { "default", "default" }),
            });
    }
}
