namespace Acme.Workforce.Orgdesign.CostModel.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a cost model entry must pass. Check returns its problems, or none.</summary>
public interface ICostModelEntryRule
{
    List<string> Check(CostModelEntry item);
}

public sealed record CostModelEntryCodePrefixRule(string Prefix) : ICostModelEntryRule
{
    public List<string> Check(CostModelEntry item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CostModelEntryAllowedOwnerIdRule(List<string> Allowed) : ICostModelEntryRule
{
    public List<string> Check(CostModelEntry item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.OwnerId)))
        {
            return new List<string> { $"owner id {item.OwnerId} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record CostModelEntryRequiredCodeRule(string Message) : ICostModelEntryRule
{
    public List<string> Check(CostModelEntry item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CostModelEntryCodeLengthRule(int MaxLength) : ICostModelEntryRule
{
    public List<string> Check(CostModelEntry item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CostModelEntryRegionLengthRule(int MaxLength) : ICostModelEntryRule
{
    public List<string> Check(CostModelEntry item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CostModelEntryChecksumLengthRule(int MaxLength) : ICostModelEntryRule
{
    public List<string> Check(CostModelEntry item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a cost model entry.</summary>
public sealed record CostModelEntryValidator(List<ICostModelEntryRule> Rules)
{
    public List<string> Validate(CostModelEntry item)
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

    public void RequireValid(CostModelEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CostModelEntry> items)
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

/// <summary>Functions of cost model entry rules.</summary>
public static class CostModelEntryRules
{
    public static CostModelEntryValidator DefaultCostModelEntryValidator()
    {
        return new CostModelEntryValidator(
            new List<ICostModelEntryRule>
            {
                new CostModelEntryCodePrefixRule("OPS"),
                new CostModelEntryAllowedOwnerIdRule(new List<string> { "primary", "lenient", "global" }),
                new CostModelEntryRequiredCodeRule("cost model entry needs an entry"),
                new CostModelEntryCodeLengthRule(80),
                new CostModelEntryRegionLengthRule(1000),
                new CostModelEntryChecksumLengthRule(64),
            });
    }
}
