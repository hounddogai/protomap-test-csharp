namespace Acme.Workforce.Payrollops.Garnishment.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a garnishment allocation must pass. Check returns its problems, or none.</summary>
public interface IGarnishmentAllocationRule
{
    List<string> Check(GarnishmentAllocation item);
}

public sealed record GarnishmentAllocationContactEmailLengthRule(int MaxLength) : IGarnishmentAllocationRule
{
    public List<string> Check(GarnishmentAllocation item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentAllocationExternalRefLengthRule(int MaxLength) : IGarnishmentAllocationRule
{
    public List<string> Check(GarnishmentAllocation item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentAllocationCodePrefixRule(string Prefix) : IGarnishmentAllocationRule
{
    public List<string> Check(GarnishmentAllocation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentAllocationTitleLengthRule(int MaxLength) : IGarnishmentAllocationRule
{
    public List<string> Check(GarnishmentAllocation item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentAllocationAllowedContactEmailRule(List<string> Allowed) : IGarnishmentAllocationRule
{
    public List<string> Check(GarnishmentAllocation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ContactEmail)))
        {
            return new List<string> { $"contact email {item.ContactEmail} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentAllocationRequiredContactEmailRule(string Message) : IGarnishmentAllocationRule
{
    public List<string> Check(GarnishmentAllocation item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a garnishment allocation.</summary>
public sealed record GarnishmentAllocationValidator(List<IGarnishmentAllocationRule> Rules)
{
    public List<string> Validate(GarnishmentAllocation item)
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

    public void RequireValid(GarnishmentAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GarnishmentAllocation> items)
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

/// <summary>Functions of garnishment allocation rules.</summary>
public static class GarnishmentAllocationRules
{
    public static GarnishmentAllocationValidator DefaultGarnishmentAllocationValidator()
    {
        return new GarnishmentAllocationValidator(
            new List<IGarnishmentAllocationRule>
            {
                new GarnishmentAllocationContactEmailLengthRule(255),
                new GarnishmentAllocationExternalRefLengthRule(120),
                new GarnishmentAllocationCodePrefixRule("ACM"),
                new GarnishmentAllocationTitleLengthRule(86400),
                new GarnishmentAllocationAllowedContactEmailRule(new List<string> { "tiered", "batched", "lenient" }),
                new GarnishmentAllocationRequiredContactEmailRule("garnishment allocation needs a value"),
            });
    }
}
