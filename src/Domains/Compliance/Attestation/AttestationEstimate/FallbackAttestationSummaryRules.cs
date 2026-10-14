namespace Acme.Workforce.Compliance.Attestation.AttestationEstimate;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a fallback attestation summary must pass. Check returns its problems, or none.</summary>
public interface IFallbackAttestationSummaryRule
{
    List<string> Check(FallbackAttestationSummary item);
}

public sealed record FallbackAttestationSummaryAllowedNameRule(List<string> Allowed) : IFallbackAttestationSummaryRule
{
    public List<string> Check(FallbackAttestationSummary item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is stale" };
        }
        return new List<string>();
    }
}

public sealed record FallbackAttestationSummaryRequiredChecksumRule(string Message) : IFallbackAttestationSummaryRule
{
    public List<string> Check(FallbackAttestationSummary item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackAttestationSummaryRequiredSourceRule(string Message) : IFallbackAttestationSummaryRule
{
    public List<string> Check(FallbackAttestationSummary item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackAttestationSummaryRequiredNameRule(string Message) : IFallbackAttestationSummaryRule
{
    public List<string> Check(FallbackAttestationSummary item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackAttestationSummarySourceLengthRule(int MaxLength) : IFallbackAttestationSummaryRule
{
    public List<string> Check(FallbackAttestationSummary item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback attestation summary.</summary>
public sealed record FallbackAttestationSummaryValidator(List<IFallbackAttestationSummaryRule> Rules)
{
    public List<string> Validate(FallbackAttestationSummary item)
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

    public void RequireValid(FallbackAttestationSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackAttestationSummary> items)
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

/// <summary>Functions of fallback attestation summary rules.</summary>
public static class FallbackAttestationSummaryRules
{
    public static FallbackAttestationSummaryValidator DefaultFallbackAttestationSummaryValidator()
    {
        return new FallbackAttestationSummaryValidator(
            new List<IFallbackAttestationSummaryRule>
            {
                new FallbackAttestationSummaryAllowedNameRule(new List<string> { "legacy", "tiered" }),
                new FallbackAttestationSummaryRequiredChecksumRule("fallback attestation summary needs a value"),
                new FallbackAttestationSummaryRequiredSourceRule("fallback attestation summary needs a value"),
                new FallbackAttestationSummaryRequiredNameRule("fallback attestation summary needs a value"),
                new FallbackAttestationSummarySourceLengthRule(255),
            });
    }
}
