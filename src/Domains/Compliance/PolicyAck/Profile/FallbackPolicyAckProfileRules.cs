namespace Acme.Workforce.Compliance.PolicyAck.Profile;

/// <summary>A rule a fallback policy ack profile must pass. Check returns its problems, or none.</summary>
public interface IFallbackPolicyAckProfileRule
{
    List<string> Check(FallbackPolicyAckProfile item);
}

public sealed record FallbackPolicyAckProfileNameLengthRule(int MaxLength) : IFallbackPolicyAckProfileRule
{
    public List<string> Check(FallbackPolicyAckProfile item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackPolicyAckProfileRequiredLocaleRule(string Message) : IFallbackPolicyAckProfileRule
{
    public List<string> Check(FallbackPolicyAckProfile item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackPolicyAckProfileRequiredNameRule(string Message) : IFallbackPolicyAckProfileRule
{
    public List<string> Check(FallbackPolicyAckProfile item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackPolicyAckProfileCodeLengthRule(int MaxLength) : IFallbackPolicyAckProfileRule
{
    public List<string> Check(FallbackPolicyAckProfile item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackPolicyAckProfileVisibleGuardRule(int Limit) : IFallbackPolicyAckProfileRule
{
    public List<string> Check(FallbackPolicyAckProfile item)
    {
        if (item.Visible && item.GraceDays > Limit)
        {
            return new List<string> { $"grace days {item.GraceDays} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback policy ack profile.</summary>
public sealed record FallbackPolicyAckProfileValidator(List<IFallbackPolicyAckProfileRule> Rules)
{
    public List<string> Validate(FallbackPolicyAckProfile item)
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

    public void RequireValid(FallbackPolicyAckProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackPolicyAckProfile> items)
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

/// <summary>Functions of fallback policy ack profile rules.</summary>
public static class FallbackPolicyAckProfileRules
{
    public static FallbackPolicyAckProfileValidator DefaultFallbackPolicyAckProfileValidator()
    {
        return new FallbackPolicyAckProfileValidator(
            new List<IFallbackPolicyAckProfileRule>
            {
                new FallbackPolicyAckProfileNameLengthRule(64),
                new FallbackPolicyAckProfileRequiredLocaleRule("fallback policy ack profile needs this field"),
                new FallbackPolicyAckProfileRequiredNameRule("fallback policy ack profile needs a value"),
                new FallbackPolicyAckProfileCodeLengthRule(32),
                new FallbackPolicyAckProfileVisibleGuardRule(255),
            });
    }
}
