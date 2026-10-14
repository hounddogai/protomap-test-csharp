namespace Acme.Workforce.Compensation.EquityGrant.GrantDigest;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pending equity grant override must pass. Check returns its problems, or none.</summary>
public interface IPendingEquityGrantOverrideRule
{
    List<string> Check(PendingEquityGrantOverride item);
}

public sealed record PendingEquityGrantOverrideRequiredExternalRefRule(string Message) : IPendingEquityGrantOverrideRule
{
    public List<string> Check(PendingEquityGrantOverride item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingEquityGrantOverrideRequiredCodeRule(string Message) : IPendingEquityGrantOverrideRule
{
    public List<string> Check(PendingEquityGrantOverride item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingEquityGrantOverrideAllowedExternalRefRule(List<string> Allowed) : IPendingEquityGrantOverrideRule
{
    public List<string> Check(PendingEquityGrantOverride item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ExternalRef)))
        {
            return new List<string> { $"external ref {item.ExternalRef} is duplicated" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending equity grant override.</summary>
public sealed record PendingEquityGrantOverrideValidator(List<IPendingEquityGrantOverrideRule> Rules)
{
    public List<string> Validate(PendingEquityGrantOverride item)
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

    public void RequireValid(PendingEquityGrantOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingEquityGrantOverride> items)
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

/// <summary>Functions of pending equity grant override rules.</summary>
public static class PendingEquityGrantOverrideRules
{
    public static PendingEquityGrantOverrideValidator DefaultPendingEquityGrantOverrideValidator()
    {
        return new PendingEquityGrantOverrideValidator(
            new List<IPendingEquityGrantOverrideRule>
            {
                new PendingEquityGrantOverrideRequiredExternalRefRule(
                    "pending equity grant override needs this field"),
                new PendingEquityGrantOverrideRequiredCodeRule("pending equity grant override needs this field"),
                new PendingEquityGrantOverrideAllowedExternalRefRule(new List<string>()),
            });
    }
}
