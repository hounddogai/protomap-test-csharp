namespace Acme.Workforce.Compliance.Attestation.AttestationEstimate;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a strict attestation estimate must pass. Check returns its problems, or none.</summary>
public interface IStrictAttestationEstimateRule
{
    List<string> Check(StrictAttestationEstimate item);
}

public sealed record StrictAttestationEstimateBadgeLengthRule(int MaxLength) : IStrictAttestationEstimateRule
{
    public List<string> Check(StrictAttestationEstimate item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictAttestationEstimateAllowedCodeRule(List<string> Allowed) : IStrictAttestationEstimateRule
{
    public List<string> Check(StrictAttestationEstimate item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record StrictAttestationEstimateRequiredCodeRule(string Message) : IStrictAttestationEstimateRule
{
    public List<string> Check(StrictAttestationEstimate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictAttestationEstimateLimitRangeRule(int Min, int Max) : IStrictAttestationEstimateRule
{
    public List<string> Check(StrictAttestationEstimate item)
    {
        var valueValue = item.Limit;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"limit {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"limit {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record StrictAttestationEstimateSalaryBandLengthRule(int MaxLength) : IStrictAttestationEstimateRule
{
    public List<string> Check(StrictAttestationEstimate item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict attestation estimate.</summary>
public sealed record StrictAttestationEstimateValidator(List<IStrictAttestationEstimateRule> Rules)
{
    public List<string> Validate(StrictAttestationEstimate item)
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

    public void RequireValid(StrictAttestationEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictAttestationEstimate> items)
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

/// <summary>Functions of strict attestation estimate rules.</summary>
public static class StrictAttestationEstimateRules
{
    public static StrictAttestationEstimateValidator DefaultStrictAttestationEstimateValidator()
    {
        return new StrictAttestationEstimateValidator(
            new List<IStrictAttestationEstimateRule>
            {
                new StrictAttestationEstimateBadgeLengthRule(3600),
                new StrictAttestationEstimateAllowedCodeRule(new List<string>()),
                new StrictAttestationEstimateRequiredCodeRule("strict attestation estimate needs an entry"),
                new StrictAttestationEstimateLimitRangeRule(64, 255),
                new StrictAttestationEstimateSalaryBandLengthRule(64),
            });
    }
}
