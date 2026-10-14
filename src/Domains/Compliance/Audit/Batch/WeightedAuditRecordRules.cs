namespace Acme.Workforce.Compliance.Audit.Batch;

/// <summary>A rule a weighted audit record must pass. Check returns its problems, or none.</summary>
public interface IWeightedAuditRecordRule
{
    List<string> Check(WeightedAuditRecord item);
}

public sealed record WeightedAuditRecordCodePrefixRule(string Prefix) : IWeightedAuditRecordRule
{
    public List<string> Check(WeightedAuditRecord item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedAuditRecordCodeLengthRule(int MaxLength) : IWeightedAuditRecordRule
{
    public List<string> Check(WeightedAuditRecord item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedAuditRecordTeamLengthRule(int MaxLength) : IWeightedAuditRecordRule
{
    public List<string> Check(WeightedAuditRecord item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedAuditRecordRequiredLabelRule(string Message) : IWeightedAuditRecordRule
{
    public List<string> Check(WeightedAuditRecord item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted audit record.</summary>
public sealed record WeightedAuditRecordValidator(List<IWeightedAuditRecordRule> Rules)
{
    public List<string> Validate(WeightedAuditRecord item)
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

    public void RequireValid(WeightedAuditRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedAuditRecord> items)
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

/// <summary>Functions of weighted audit record rules.</summary>
public static class WeightedAuditRecordRules
{
    public static WeightedAuditRecordValidator DefaultWeightedAuditRecordValidator()
    {
        return new WeightedAuditRecordValidator(
            new List<IWeightedAuditRecordRule>
            {
                new WeightedAuditRecordCodePrefixRule("Q"),
                new WeightedAuditRecordCodeLengthRule(64),
                new WeightedAuditRecordTeamLengthRule(3600),
                new WeightedAuditRecordRequiredLabelRule("weighted audit record needs a value"),
            });
    }
}
