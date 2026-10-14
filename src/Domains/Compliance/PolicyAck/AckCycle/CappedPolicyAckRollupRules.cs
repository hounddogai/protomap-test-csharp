namespace Acme.Workforce.Compliance.PolicyAck.AckCycle;

/// <summary>A rule a capped policy ack rollup must pass. Check returns its problems, or none.</summary>
public interface ICappedPolicyAckRollupRule
{
    List<string> Check(CappedPolicyAckRollup item);
}

public sealed record CappedPolicyAckRollupRequiredSummaryRule(string Message) : ICappedPolicyAckRollupRule
{
    public List<string> Check(CappedPolicyAckRollup item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedPolicyAckRollupCodeLengthRule(int MaxLength) : ICappedPolicyAckRollupRule
{
    public List<string> Check(CappedPolicyAckRollup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedPolicyAckRollupRequiredSegmentRule(string Message) : ICappedPolicyAckRollupRule
{
    public List<string> Check(CappedPolicyAckRollup item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedPolicyAckRollupRequiredTitleRule(string Message) : ICappedPolicyAckRollupRule
{
    public List<string> Check(CappedPolicyAckRollup item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedPolicyAckRollupSummaryLengthRule(int MaxLength) : ICappedPolicyAckRollupRule
{
    public List<string> Check(CappedPolicyAckRollup item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped policy ack rollup.</summary>
public sealed record CappedPolicyAckRollupValidator(List<ICappedPolicyAckRollupRule> Rules)
{
    public List<string> Validate(CappedPolicyAckRollup item)
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

    public void RequireValid(CappedPolicyAckRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedPolicyAckRollup> items)
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

/// <summary>Functions of capped policy ack rollup rules.</summary>
public static class CappedPolicyAckRollupRules
{
    public static CappedPolicyAckRollupValidator DefaultCappedPolicyAckRollupValidator()
    {
        return new CappedPolicyAckRollupValidator(
            new List<ICappedPolicyAckRollupRule>
            {
                new CappedPolicyAckRollupRequiredSummaryRule("capped policy ack rollup needs an entry"),
                new CappedPolicyAckRollupCodeLengthRule(32),
                new CappedPolicyAckRollupRequiredSegmentRule("capped policy ack rollup needs an entry"),
                new CappedPolicyAckRollupRequiredTitleRule("capped policy ack rollup needs an entry"),
                new CappedPolicyAckRollupSummaryLengthRule(255),
            });
    }
}
