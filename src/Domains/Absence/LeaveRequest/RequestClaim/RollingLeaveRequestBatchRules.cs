namespace Acme.Workforce.Absence.LeaveRequest.RequestClaim;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rolling leave request batch must pass. Check returns its problems, or none.</summary>
public interface IRollingLeaveRequestBatchRule
{
    List<string> Check(RollingLeaveRequestBatch item);
}

public sealed record RollingLeaveRequestBatchRequiredLabelRule(string Message) : IRollingLeaveRequestBatchRule
{
    public List<string> Check(RollingLeaveRequestBatch item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingLeaveRequestBatchRequiredBadgeRule(string Message) : IRollingLeaveRequestBatchRule
{
    public List<string> Check(RollingLeaveRequestBatch item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingLeaveRequestBatchLabelLengthRule(int MaxLength) : IRollingLeaveRequestBatchRule
{
    public List<string> Check(RollingLeaveRequestBatch item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingLeaveRequestBatchAllowedUnitRule(List<string> Allowed) : IRollingLeaveRequestBatchRule
{
    public List<string> Check(RollingLeaveRequestBatch item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Unit)))
        {
            return new List<string> { $"unit {item.Unit} is expired" };
        }
        return new List<string>();
    }
}

public sealed record RollingLeaveRequestBatchCodePrefixRule(string Prefix) : IRollingLeaveRequestBatchRule
{
    public List<string> Check(RollingLeaveRequestBatch item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling leave request batch.</summary>
public sealed record RollingLeaveRequestBatchValidator(List<IRollingLeaveRequestBatchRule> Rules)
{
    public List<string> Validate(RollingLeaveRequestBatch item)
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

    public void RequireValid(RollingLeaveRequestBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingLeaveRequestBatch> items)
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

/// <summary>Functions of rolling leave request batch rules.</summary>
public static class RollingLeaveRequestBatchRules
{
    public static RollingLeaveRequestBatchValidator DefaultRollingLeaveRequestBatchValidator()
    {
        return new RollingLeaveRequestBatchValidator(
            new List<IRollingLeaveRequestBatchRule>
            {
                new RollingLeaveRequestBatchRequiredLabelRule("rolling leave request batch needs an entry"),
                new RollingLeaveRequestBatchRequiredBadgeRule("rolling leave request batch needs a value"),
                new RollingLeaveRequestBatchLabelLengthRule(64),
                new RollingLeaveRequestBatchAllowedUnitRule(new List<string> { "capped", "weighted" }),
                new RollingLeaveRequestBatchCodePrefixRule("OPS"),
            });
    }
}
