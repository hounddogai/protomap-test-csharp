namespace Acme.Workforce.Compliance.Attestation.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a adaptive attestation record must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveAttestationRecordRule
{
    List<string> Check(AdaptiveAttestationRecord item);
}

public sealed record AdaptiveAttestationRecordAllowedTimezoneRule(List<string> Allowed) : IAdaptiveAttestationRecordRule
{
    public List<string> Check(AdaptiveAttestationRecord item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Timezone)))
        {
            return new List<string> { $"timezone {item.Timezone} is too long" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveAttestationRecordRequiredTimezoneRule(string Message) : IAdaptiveAttestationRecordRule
{
    public List<string> Check(AdaptiveAttestationRecord item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveAttestationRecordRequiredCodeRule(string Message) : IAdaptiveAttestationRecordRule
{
    public List<string> Check(AdaptiveAttestationRecord item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveAttestationRecordLabelLengthRule(int MaxLength) : IAdaptiveAttestationRecordRule
{
    public List<string> Check(AdaptiveAttestationRecord item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive attestation record.</summary>
public sealed record AdaptiveAttestationRecordValidator(List<IAdaptiveAttestationRecordRule> Rules)
{
    public List<string> Validate(AdaptiveAttestationRecord item)
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

    public void RequireValid(AdaptiveAttestationRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveAttestationRecord> items)
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

/// <summary>Functions of adaptive attestation record rules.</summary>
public static class AdaptiveAttestationRecordRules
{
    public static AdaptiveAttestationRecordValidator DefaultAdaptiveAttestationRecordValidator()
    {
        return new AdaptiveAttestationRecordValidator(
            new List<IAdaptiveAttestationRecordRule>
            {
                new AdaptiveAttestationRecordAllowedTimezoneRule(new List<string> { "tiered", "tiered" }),
                new AdaptiveAttestationRecordRequiredTimezoneRule("adaptive attestation record needs this field"),
                new AdaptiveAttestationRecordRequiredCodeRule("adaptive attestation record needs an entry"),
                new AdaptiveAttestationRecordLabelLengthRule(64),
            });
    }
}
