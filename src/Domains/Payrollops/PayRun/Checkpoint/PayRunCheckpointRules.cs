namespace Acme.Workforce.Payrollops.PayRun.Checkpoint;

/// <summary>A rule a pay run checkpoint must pass. Check returns its problems, or none.</summary>
public interface IPayRunCheckpointRule
{
    List<string> Check(PayRunCheckpoint item);
}

public sealed record PayRunCheckpointRequiredLabelRule(string Message) : IPayRunCheckpointRule
{
    public List<string> Check(PayRunCheckpoint item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayRunCheckpointCodePrefixRule(string Prefix) : IPayRunCheckpointRule
{
    public List<string> Check(PayRunCheckpoint item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunCheckpointRequiredPhoneRule(string Message) : IPayRunCheckpointRule
{
    public List<string> Check(PayRunCheckpoint item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayRunCheckpointTeamLengthRule(int MaxLength) : IPayRunCheckpointRule
{
    public List<string> Check(PayRunCheckpoint item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunCheckpointCodeLengthRule(int MaxLength) : IPayRunCheckpointRule
{
    public List<string> Check(PayRunCheckpoint item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay run checkpoint.</summary>
public sealed record PayRunCheckpointValidator(List<IPayRunCheckpointRule> Rules)
{
    public List<string> Validate(PayRunCheckpoint item)
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

    public void RequireValid(PayRunCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayRunCheckpoint> items)
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

/// <summary>Functions of pay run checkpoint rules.</summary>
public static class PayRunCheckpointRules
{
    public static PayRunCheckpointValidator DefaultPayRunCheckpointValidator()
    {
        return new PayRunCheckpointValidator(
            new List<IPayRunCheckpointRule>
            {
                new PayRunCheckpointRequiredLabelRule("pay run checkpoint needs an entry"),
                new PayRunCheckpointCodePrefixRule("Q"),
                new PayRunCheckpointRequiredPhoneRule("pay run checkpoint needs an entry"),
                new PayRunCheckpointTeamLengthRule(120),
                new PayRunCheckpointCodeLengthRule(86400),
            });
    }
}
