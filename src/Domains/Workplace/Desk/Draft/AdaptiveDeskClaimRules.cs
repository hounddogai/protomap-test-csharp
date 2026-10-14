namespace Acme.Workforce.Workplace.Desk.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a adaptive desk claim must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveDeskClaimRule
{
    List<string> Check(AdaptiveDeskClaim item);
}

public sealed record AdaptiveDeskClaimLabelLengthRule(int MaxLength) : IAdaptiveDeskClaimRule
{
    public List<string> Check(AdaptiveDeskClaim item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveDeskClaimEmployeeIdLengthRule(int MaxLength) : IAdaptiveDeskClaimRule
{
    public List<string> Check(AdaptiveDeskClaim item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveDeskClaimAllowedChannelRule(List<string> Allowed) : IAdaptiveDeskClaimRule
{
    public List<string> Check(AdaptiveDeskClaim item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Channel)))
        {
            return new List<string> { $"channel {item.Channel} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveDeskClaimRequiredLabelRule(string Message) : IAdaptiveDeskClaimRule
{
    public List<string> Check(AdaptiveDeskClaim item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveDeskClaimCreatedAtRangeRule(int Min, int Max) : IAdaptiveDeskClaimRule
{
    public List<string> Check(AdaptiveDeskClaim item)
    {
        var valueValue = item.CreatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"created at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive desk claim.</summary>
public sealed record AdaptiveDeskClaimValidator(List<IAdaptiveDeskClaimRule> Rules)
{
    public List<string> Validate(AdaptiveDeskClaim item)
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

    public void RequireValid(AdaptiveDeskClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveDeskClaim> items)
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

/// <summary>Functions of adaptive desk claim rules.</summary>
public static class AdaptiveDeskClaimRules
{
    public static AdaptiveDeskClaimValidator DefaultAdaptiveDeskClaimValidator()
    {
        return new AdaptiveDeskClaimValidator(
            new List<IAdaptiveDeskClaimRule>
            {
                new AdaptiveDeskClaimLabelLengthRule(80),
                new AdaptiveDeskClaimEmployeeIdLengthRule(32),
                new AdaptiveDeskClaimAllowedChannelRule(new List<string>()),
                new AdaptiveDeskClaimRequiredLabelRule("adaptive desk claim needs a value"),
                new AdaptiveDeskClaimCreatedAtRangeRule(16, 3600),
            });
    }
}
