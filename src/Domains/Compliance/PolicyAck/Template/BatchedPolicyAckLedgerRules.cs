namespace Acme.Workforce.Compliance.PolicyAck.Template;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a batched policy ack ledger must pass. Check returns its problems, or none.</summary>
public interface IBatchedPolicyAckLedgerRule
{
    List<string> Check(BatchedPolicyAckLedger item);
}

public sealed record BatchedPolicyAckLedgerVersionRangeRule(int Min, int Max) : IBatchedPolicyAckLedgerRule
{
    public List<string> Check(BatchedPolicyAckLedger item)
    {
        var valueValue = item.Version;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"version {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"version {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BatchedPolicyAckLedgerCodeLengthRule(int MaxLength) : IBatchedPolicyAckLedgerRule
{
    public List<string> Check(BatchedPolicyAckLedger item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedPolicyAckLedgerAllowedCodeRule(List<string> Allowed) : IBatchedPolicyAckLedgerRule
{
    public List<string> Check(BatchedPolicyAckLedger item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record BatchedPolicyAckLedgerFteRangeRule(double Min, double Max) : IBatchedPolicyAckLedgerRule
{
    public List<string> Check(BatchedPolicyAckLedger item)
    {
        var valueValue = item.Fte;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"fte {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"fte {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BatchedPolicyAckLedgerTitleLengthRule(int MaxLength) : IBatchedPolicyAckLedgerRule
{
    public List<string> Check(BatchedPolicyAckLedger item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched policy ack ledger.</summary>
public sealed record BatchedPolicyAckLedgerValidator(List<IBatchedPolicyAckLedgerRule> Rules)
{
    public List<string> Validate(BatchedPolicyAckLedger item)
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

    public void RequireValid(BatchedPolicyAckLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedPolicyAckLedger> items)
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

/// <summary>Functions of batched policy ack ledger rules.</summary>
public static class BatchedPolicyAckLedgerRules
{
    public static BatchedPolicyAckLedgerValidator DefaultBatchedPolicyAckLedgerValidator()
    {
        return new BatchedPolicyAckLedgerValidator(
            new List<IBatchedPolicyAckLedgerRule>
            {
                new BatchedPolicyAckLedgerVersionRangeRule(3600, 3600),
                new BatchedPolicyAckLedgerCodeLengthRule(3600),
                new BatchedPolicyAckLedgerAllowedCodeRule(new List<string> { "manual", "lenient", "capped" }),
                new BatchedPolicyAckLedgerFteRangeRule(0.8, 1886.4),
                new BatchedPolicyAckLedgerTitleLengthRule(120),
            });
    }
}
