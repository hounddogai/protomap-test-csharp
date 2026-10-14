namespace Acme.Workforce.Compliance.Attestation.RecordItem;

/// <summary>A rule a default attestation rollup must pass. Check returns its problems, or none.</summary>
public interface IDefaultAttestationRollupRule
{
    List<string> Check(DefaultAttestationRollup item);
}

public sealed record DefaultAttestationRollupRequiredLabelRule(string Message) : IDefaultAttestationRollupRule
{
    public List<string> Check(DefaultAttestationRollup item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultAttestationRollupLabelLengthRule(int MaxLength) : IDefaultAttestationRollupRule
{
    public List<string> Check(DefaultAttestationRollup item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultAttestationRollupCodeLengthRule(int MaxLength) : IDefaultAttestationRollupRule
{
    public List<string> Check(DefaultAttestationRollup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default attestation rollup.</summary>
public sealed record DefaultAttestationRollupValidator(List<IDefaultAttestationRollupRule> Rules)
{
    public List<string> Validate(DefaultAttestationRollup item)
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

    public void RequireValid(DefaultAttestationRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultAttestationRollup> items)
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

/// <summary>Functions of default attestation rollup rules.</summary>
public static class DefaultAttestationRollupRules
{
    public static DefaultAttestationRollupValidator DefaultDefaultAttestationRollupValidator()
    {
        return new DefaultAttestationRollupValidator(
            new List<IDefaultAttestationRollupRule>
            {
                new DefaultAttestationRollupRequiredLabelRule("default attestation rollup needs an entry"),
                new DefaultAttestationRollupLabelLengthRule(16),
                new DefaultAttestationRollupCodeLengthRule(16),
            });
    }
}
