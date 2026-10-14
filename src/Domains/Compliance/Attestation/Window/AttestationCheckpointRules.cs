namespace Acme.Workforce.Compliance.Attestation.Window;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a attestation checkpoint must pass. Check returns its problems, or none.</summary>
public interface IAttestationCheckpointRule
{
    List<string> Check(AttestationCheckpoint item);
}

public sealed record AttestationCheckpointCodeLengthRule(int MaxLength) : IAttestationCheckpointRule
{
    public List<string> Check(AttestationCheckpoint item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AttestationCheckpointCodePrefixRule(string Prefix) : IAttestationCheckpointRule
{
    public List<string> Check(AttestationCheckpoint item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AttestationCheckpointAllowedCurrencyRule(List<string> Allowed) : IAttestationCheckpointRule
{
    public List<string> Check(AttestationCheckpoint item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Currency)))
        {
            return new List<string> { $"currency {item.Currency} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record AttestationCheckpointLabelLengthRule(int MaxLength) : IAttestationCheckpointRule
{
    public List<string> Check(AttestationCheckpoint item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AttestationCheckpointBudgetRangeRule(double Min, double Max) : IAttestationCheckpointRule
{
    public List<string> Check(AttestationCheckpoint item)
    {
        var valueValue = item.Budget;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"budget {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"budget {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AttestationCheckpointVisibleGuardRule(double Limit) : IAttestationCheckpointRule
{
    public List<string> Check(AttestationCheckpoint item)
    {
        if (item.Visible && item.FloorValue > Limit)
        {
            return new List<string> { $"floor value {item.FloorValue} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a attestation checkpoint.</summary>
public sealed record AttestationCheckpointValidator(List<IAttestationCheckpointRule> Rules)
{
    public List<string> Validate(AttestationCheckpoint item)
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

    public void RequireValid(AttestationCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AttestationCheckpoint> items)
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

/// <summary>Functions of attestation checkpoint rules.</summary>
public static class AttestationCheckpointRules
{
    public static AttestationCheckpointValidator DefaultAttestationCheckpointValidator()
    {
        return new AttestationCheckpointValidator(
            new List<IAttestationCheckpointRule>
            {
                new AttestationCheckpointCodeLengthRule(32),
                new AttestationCheckpointCodePrefixRule("OPS"),
                new AttestationCheckpointAllowedCurrencyRule(
                    new List<string> { "scheduled", "regional", "scheduled" }),
                new AttestationCheckpointLabelLengthRule(1000),
                new AttestationCheckpointBudgetRangeRule(1.7, 970.4),
                new AttestationCheckpointVisibleGuardRule(5.3),
            });
    }
}
