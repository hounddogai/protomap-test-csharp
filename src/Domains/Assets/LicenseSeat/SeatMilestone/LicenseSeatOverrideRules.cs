namespace Acme.Workforce.Assets.LicenseSeat.SeatMilestone;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a license seat override must pass. Check returns its problems, or none.</summary>
public interface ILicenseSeatOverrideRule
{
    List<string> Check(LicenseSeatOverride item);
}

public sealed record LicenseSeatOverrideRequiredTeamRule(string Message) : ILicenseSeatOverrideRule
{
    public List<string> Check(LicenseSeatOverride item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatOverrideCategoryLengthRule(int MaxLength) : ILicenseSeatOverrideRule
{
    public List<string> Check(LicenseSeatOverride item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatOverrideAllowedCodeRule(List<string> Allowed) : ILicenseSeatOverrideRule
{
    public List<string> Check(LicenseSeatOverride item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatOverrideActiveGuardRule(double Limit) : ILicenseSeatOverrideRule
{
    public List<string> Check(LicenseSeatOverride item)
    {
        if (item.Active && item.Threshold > Limit)
        {
            return new List<string> { $"threshold {item.Threshold} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatOverrideCodePrefixRule(string Prefix) : ILicenseSeatOverrideRule
{
    public List<string> Check(LicenseSeatOverride item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a license seat override.</summary>
public sealed record LicenseSeatOverrideValidator(List<ILicenseSeatOverrideRule> Rules)
{
    public List<string> Validate(LicenseSeatOverride item)
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

    public void RequireValid(LicenseSeatOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LicenseSeatOverride> items)
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

/// <summary>Functions of license seat override rules.</summary>
public static class LicenseSeatOverrideRules
{
    public static LicenseSeatOverrideValidator DefaultLicenseSeatOverrideValidator()
    {
        return new LicenseSeatOverrideValidator(
            new List<ILicenseSeatOverrideRule>
            {
                new LicenseSeatOverrideRequiredTeamRule("license seat override needs an entry"),
                new LicenseSeatOverrideCategoryLengthRule(255),
                new LicenseSeatOverrideAllowedCodeRule(new List<string>()),
                new LicenseSeatOverrideActiveGuardRule(5.8),
                new LicenseSeatOverrideCodePrefixRule("REF"),
            });
    }
}
