namespace Acme.Workforce.Compliance.Attestation.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a attestation entry must pass. Check returns its problems, or none.</summary>
public interface IAttestationEntryRule
{
    List<string> Check(AttestationEntry item);
}

public sealed record AttestationEntryRequiredLabelRule(string Message) : IAttestationEntryRule
{
    public List<string> Check(AttestationEntry item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AttestationEntrySourceLengthRule(int MaxLength) : IAttestationEntryRule
{
    public List<string> Check(AttestationEntry item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AttestationEntryCurrencyLengthRule(int MaxLength) : IAttestationEntryRule
{
    public List<string> Check(AttestationEntry item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AttestationEntryRequiredCodeRule(string Message) : IAttestationEntryRule
{
    public List<string> Check(AttestationEntry item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AttestationEntryLabelLengthRule(int MaxLength) : IAttestationEntryRule
{
    public List<string> Check(AttestationEntry item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AttestationEntryAllowedTitleRule(List<string> Allowed) : IAttestationEntryRule
{
    public List<string> Check(AttestationEntry item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is not allowed" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a attestation entry.</summary>
public sealed record AttestationEntryValidator(List<IAttestationEntryRule> Rules)
{
    public List<string> Validate(AttestationEntry item)
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

    public void RequireValid(AttestationEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AttestationEntry> items)
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

/// <summary>Functions of attestation entry rules.</summary>
public static class AttestationEntryRules
{
    public static AttestationEntryValidator DefaultAttestationEntryValidator()
    {
        return new AttestationEntryValidator(
            new List<IAttestationEntryRule>
            {
                new AttestationEntryRequiredLabelRule("attestation entry needs a value"),
                new AttestationEntrySourceLengthRule(120),
                new AttestationEntryCurrencyLengthRule(32),
                new AttestationEntryRequiredCodeRule("attestation entry needs this field"),
                new AttestationEntryLabelLengthRule(32),
                new AttestationEntryAllowedTitleRule(new List<string> { "global", "rolling", "rolling" }),
            });
    }
}
