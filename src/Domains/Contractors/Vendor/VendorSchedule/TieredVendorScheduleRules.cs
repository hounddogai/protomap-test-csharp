namespace Acme.Workforce.Contractors.Vendor.VendorSchedule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a tiered vendor schedule must pass. Check returns its problems, or none.</summary>
public interface ITieredVendorScheduleRule
{
    List<string> Check(TieredVendorSchedule item);
}

public sealed record TieredVendorScheduleCodePrefixRule(string Prefix) : ITieredVendorScheduleRule
{
    public List<string> Check(TieredVendorSchedule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TieredVendorScheduleRequiredLabelRule(string Message) : ITieredVendorScheduleRule
{
    public List<string> Check(TieredVendorSchedule item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredVendorScheduleCodeLengthRule(int MaxLength) : ITieredVendorScheduleRule
{
    public List<string> Check(TieredVendorSchedule item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredVendorScheduleAllowedLabelRule(List<string> Allowed) : ITieredVendorScheduleRule
{
    public List<string> Check(TieredVendorSchedule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is expired" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered vendor schedule.</summary>
public sealed record TieredVendorScheduleValidator(List<ITieredVendorScheduleRule> Rules)
{
    public List<string> Validate(TieredVendorSchedule item)
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

    public void RequireValid(TieredVendorSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredVendorSchedule> items)
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

/// <summary>Functions of tiered vendor schedule rules.</summary>
public static class TieredVendorScheduleRules
{
    public static TieredVendorScheduleValidator DefaultTieredVendorScheduleValidator()
    {
        return new TieredVendorScheduleValidator(
            new List<ITieredVendorScheduleRule>
            {
                new TieredVendorScheduleCodePrefixRule("OPS"),
                new TieredVendorScheduleRequiredLabelRule("tiered vendor schedule needs this field"),
                new TieredVendorScheduleCodeLengthRule(120),
                new TieredVendorScheduleAllowedLabelRule(new List<string>()),
            });
    }
}
