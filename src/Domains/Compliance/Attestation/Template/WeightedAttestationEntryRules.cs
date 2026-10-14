namespace Acme.Workforce.Compliance.Attestation.Template;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a weighted attestation entry must pass. Check returns its problems, or none.</summary>
public interface IWeightedAttestationEntryRule
{
    List<string> Check(WeightedAttestationEntry item);
}

public sealed record WeightedAttestationEntryCodeLengthRule(int MaxLength) : IWeightedAttestationEntryRule
{
    public List<string> Check(WeightedAttestationEntry item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedAttestationEntryAllowedSegmentRule(List<string> Allowed) : IWeightedAttestationEntryRule
{
    public List<string> Check(WeightedAttestationEntry item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Segment)))
        {
            return new List<string> { $"segment {item.Segment} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record WeightedAttestationEntryLabelLengthRule(int MaxLength) : IWeightedAttestationEntryRule
{
    public List<string> Check(WeightedAttestationEntry item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted attestation entry.</summary>
public sealed record WeightedAttestationEntryValidator(List<IWeightedAttestationEntryRule> Rules)
{
    public List<string> Validate(WeightedAttestationEntry item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            foreach (var problem in rule.Check(item))
            {
                problems.Add(problem);
            }
        }
        return problems;
    }

    public void RequireValid(WeightedAttestationEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedAttestationEntry> items)
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

/// <summary>Functions of weighted attestation entry rules.</summary>
public static class WeightedAttestationEntryRules
{
    public static WeightedAttestationEntryValidator DefaultWeightedAttestationEntryValidator()
    {
        return new WeightedAttestationEntryValidator(
            new List<IWeightedAttestationEntryRule>
            {
                new WeightedAttestationEntryCodeLengthRule(16),
                new WeightedAttestationEntryAllowedSegmentRule(new List<string> { "fallback", "tiered", "default" }),
                new WeightedAttestationEntryLabelLengthRule(120),
            });
    }
}
