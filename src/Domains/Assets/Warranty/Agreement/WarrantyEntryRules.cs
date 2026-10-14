namespace Acme.Workforce.Assets.Warranty.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a warranty entry must pass. Check returns its problems, or none.</summary>
public interface IWarrantyEntryRule
{
    List<string> Check(WarrantyEntry item);
}

public sealed record WarrantyEntryCodePrefixRule(string Prefix) : IWarrantyEntryRule
{
    public List<string> Check(WarrantyEntry item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyEntrySourceLengthRule(int MaxLength) : IWarrantyEntryRule
{
    public List<string> Check(WarrantyEntry item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyEntryAllowedEmployeeIdRule(List<string> Allowed) : IWarrantyEntryRule
{
    public List<string> Check(WarrantyEntry item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.EmployeeId)))
        {
            return new List<string> { $"employee id {item.EmployeeId} is expired" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a warranty entry.</summary>
public sealed record WarrantyEntryValidator(List<IWarrantyEntryRule> Rules)
{
    public List<string> Validate(WarrantyEntry item)
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

    public void RequireValid(WarrantyEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WarrantyEntry> items)
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

/// <summary>Functions of warranty entry rules.</summary>
public static class WarrantyEntryRules
{
    public static WarrantyEntryValidator DefaultWarrantyEntryValidator()
    {
        return new WarrantyEntryValidator(
            new List<IWarrantyEntryRule>
            {
                new WarrantyEntryCodePrefixRule("REF"),
                new WarrantyEntrySourceLengthRule(80),
                new WarrantyEntryAllowedEmployeeIdRule(new List<string> { "rolling" }),
            });
    }
}
