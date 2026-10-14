namespace Acme.Workforce.Payrollops.PayRun.RunRecord;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pay run estimate must pass. Check returns its problems, or none.</summary>
public interface IPayRunEstimateRule
{
    List<string> Check(PayRunEstimate item);
}

public sealed record PayRunEstimateNameLengthRule(int MaxLength) : IPayRunEstimateRule
{
    public List<string> Check(PayRunEstimate item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunEstimateRequiredCodeRule(string Message) : IPayRunEstimateRule
{
    public List<string> Check(PayRunEstimate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayRunEstimateTeamLengthRule(int MaxLength) : IPayRunEstimateRule
{
    public List<string> Check(PayRunEstimate item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunEstimateAllowedSegmentRule(List<string> Allowed) : IPayRunEstimateRule
{
    public List<string> Check(PayRunEstimate item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Segment)))
        {
            return new List<string> { $"segment {item.Segment} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record PayRunEstimateRequiredTeamRule(string Message) : IPayRunEstimateRule
{
    public List<string> Check(PayRunEstimate item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay run estimate.</summary>
public sealed record PayRunEstimateValidator(List<IPayRunEstimateRule> Rules)
{
    public List<string> Validate(PayRunEstimate item)
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

    public void RequireValid(PayRunEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayRunEstimate> items)
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

/// <summary>Functions of pay run estimate rules.</summary>
public static class PayRunEstimateRules
{
    public static PayRunEstimateValidator DefaultPayRunEstimateValidator()
    {
        return new PayRunEstimateValidator(
            new List<IPayRunEstimateRule>
            {
                new PayRunEstimateNameLengthRule(16),
                new PayRunEstimateRequiredCodeRule("pay run estimate needs this field"),
                new PayRunEstimateTeamLengthRule(16),
                new PayRunEstimateAllowedSegmentRule(new List<string>()),
                new PayRunEstimateRequiredTeamRule("pay run estimate needs an entry"),
            });
    }
}
