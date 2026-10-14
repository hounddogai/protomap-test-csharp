namespace Acme.Workforce.Talent.Mobility.MobilityMapping;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scheduled mobility mapping must pass. Check returns its problems, or none.</summary>
public interface IScheduledMobilityMappingRule
{
    List<string> Check(ScheduledMobilityMapping item);
}

public sealed record ScheduledMobilityMappingAllowedChecksumRule(List<string> Allowed) : IScheduledMobilityMappingRule
{
    public List<string> Check(ScheduledMobilityMapping item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Checksum)))
        {
            return new List<string> { $"checksum {item.Checksum} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledMobilityMappingRequiredNameRule(string Message) : IScheduledMobilityMappingRule
{
    public List<string> Check(ScheduledMobilityMapping item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledMobilityMappingQuantityRangeRule(int Min, int Max) : IScheduledMobilityMappingRule
{
    public List<string> Check(ScheduledMobilityMapping item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledMobilityMappingCodePrefixRule(string Prefix) : IScheduledMobilityMappingRule
{
    public List<string> Check(ScheduledMobilityMapping item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledMobilityMappingRequiredChecksumRule(string Message) : IScheduledMobilityMappingRule
{
    public List<string> Check(ScheduledMobilityMapping item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled mobility mapping.</summary>
public sealed record ScheduledMobilityMappingValidator(List<IScheduledMobilityMappingRule> Rules)
{
    public List<string> Validate(ScheduledMobilityMapping item)
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

    public void RequireValid(ScheduledMobilityMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledMobilityMapping> items)
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

/// <summary>Functions of scheduled mobility mapping rules.</summary>
public static class ScheduledMobilityMappingRules
{
    public static ScheduledMobilityMappingValidator DefaultScheduledMobilityMappingValidator()
    {
        return new ScheduledMobilityMappingValidator(
            new List<IScheduledMobilityMappingRule>
            {
                new ScheduledMobilityMappingAllowedChecksumRule(new List<string> { "batched", "manual", "strict" }),
                new ScheduledMobilityMappingRequiredNameRule("scheduled mobility mapping needs an entry"),
                new ScheduledMobilityMappingQuantityRangeRule(80, 120),
                new ScheduledMobilityMappingCodePrefixRule("X"),
                new ScheduledMobilityMappingRequiredChecksumRule("scheduled mobility mapping needs this field"),
            });
    }
}
