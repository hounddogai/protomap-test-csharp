namespace Acme.Workforce.Compliance.Attestation.Checkpoint;

/// <summary>A rule a global attestation checkpoint must pass. Check returns its problems, or none.</summary>
public interface IGlobalAttestationCheckpointRule
{
    List<string> Check(GlobalAttestationCheckpoint item);
}

public sealed record GlobalAttestationCheckpointCodePrefixRule(string Prefix) : IGlobalAttestationCheckpointRule
{
    public List<string> Check(GlobalAttestationCheckpoint item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalAttestationCheckpointStartsAtRangeRule(int Min, int Max) : IGlobalAttestationCheckpointRule
{
    public List<string> Check(GlobalAttestationCheckpoint item)
    {
        var valueValue = item.StartsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"starts at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalAttestationCheckpointAttemptsRangeRule(int Min, int Max) : IGlobalAttestationCheckpointRule
{
    public List<string> Check(GlobalAttestationCheckpoint item)
    {
        var valueValue = item.Attempts;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"attempts {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalAttestationCheckpointCurrencyLengthRule(int MaxLength) : IGlobalAttestationCheckpointRule
{
    public List<string> Check(GlobalAttestationCheckpoint item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global attestation checkpoint.</summary>
public sealed record GlobalAttestationCheckpointValidator(List<IGlobalAttestationCheckpointRule> Rules)
{
    public List<string> Validate(GlobalAttestationCheckpoint item)
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

    public void RequireValid(GlobalAttestationCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalAttestationCheckpoint> items)
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

/// <summary>Functions of global attestation checkpoint rules.</summary>
public static class GlobalAttestationCheckpointRules
{
    public static GlobalAttestationCheckpointValidator DefaultGlobalAttestationCheckpointValidator()
    {
        return new GlobalAttestationCheckpointValidator(
            new List<IGlobalAttestationCheckpointRule>
            {
                new GlobalAttestationCheckpointCodePrefixRule("ACM"),
                new GlobalAttestationCheckpointStartsAtRangeRule(64, 255),
                new GlobalAttestationCheckpointAttemptsRangeRule(255, 1000),
                new GlobalAttestationCheckpointCurrencyLengthRule(32),
            });
    }
}
