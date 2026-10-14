namespace Acme.Workforce.Contractors.Vendor.DefaultVendorSchedule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a weighted vendor schedule must pass. Check returns its problems, or none.</summary>
public interface IWeightedVendorScheduleRule
{
    List<string> Check(WeightedVendorSchedule item);
}

public sealed record WeightedVendorScheduleCodePrefixRule(string Prefix) : IWeightedVendorScheduleRule
{
    public List<string> Check(WeightedVendorSchedule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedVendorScheduleRequiredNameRule(string Message) : IWeightedVendorScheduleRule
{
    public List<string> Check(WeightedVendorSchedule item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedVendorScheduleAllowedNameRule(List<string> Allowed) : IWeightedVendorScheduleRule
{
    public List<string> Check(WeightedVendorSchedule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is expired" };
        }
        return new List<string>();
    }
}

public sealed record WeightedVendorScheduleBatchSizeRangeRule(int Min, int Max) : IWeightedVendorScheduleRule
{
    public List<string> Check(WeightedVendorSchedule item)
    {
        var valueValue = item.BatchSize;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"batch size {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted vendor schedule.</summary>
public sealed record WeightedVendorScheduleValidator(List<IWeightedVendorScheduleRule> Rules)
{
    public List<string> Validate(WeightedVendorSchedule item)
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

    public void RequireValid(WeightedVendorSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedVendorSchedule> items)
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

/// <summary>Functions of weighted vendor schedule rules.</summary>
public static class WeightedVendorScheduleRules
{
    public static WeightedVendorScheduleValidator DefaultWeightedVendorScheduleValidator()
    {
        return new WeightedVendorScheduleValidator(
            new List<IWeightedVendorScheduleRule>
            {
                new WeightedVendorScheduleCodePrefixRule("X"),
                new WeightedVendorScheduleRequiredNameRule("weighted vendor schedule needs a value"),
                new WeightedVendorScheduleAllowedNameRule(new List<string> { "primary", "primary", "capped" }),
                new WeightedVendorScheduleBatchSizeRangeRule(120, 86400),
            });
    }
}
