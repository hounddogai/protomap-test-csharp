namespace Acme.Workforce.Workplace.Occupancy.Adjustment;

/// <summary>A rule a capped occupancy policy must pass. Check returns its problems, or none.</summary>
public interface ICappedOccupancyPolicyRule
{
    List<string> Check(CappedOccupancyPolicy item);
}

public sealed record CappedOccupancyPolicySummaryLengthRule(int MaxLength) : ICappedOccupancyPolicyRule
{
    public List<string> Check(CappedOccupancyPolicy item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedOccupancyPolicyNameLengthRule(int MaxLength) : ICappedOccupancyPolicyRule
{
    public List<string> Check(CappedOccupancyPolicy item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedOccupancyPolicyRequiredChannelRule(string Message) : ICappedOccupancyPolicyRule
{
    public List<string> Check(CappedOccupancyPolicy item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedOccupancyPolicyRequiredCodeRule(string Message) : ICappedOccupancyPolicyRule
{
    public List<string> Check(CappedOccupancyPolicy item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped occupancy policy.</summary>
public sealed record CappedOccupancyPolicyValidator(List<ICappedOccupancyPolicyRule> Rules)
{
    public List<string> Validate(CappedOccupancyPolicy item)
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

    public void RequireValid(CappedOccupancyPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedOccupancyPolicy> items)
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

/// <summary>Functions of capped occupancy policy rules.</summary>
public static class CappedOccupancyPolicyRules
{
    public static CappedOccupancyPolicyValidator DefaultCappedOccupancyPolicyValidator()
    {
        return new CappedOccupancyPolicyValidator(
            new List<ICappedOccupancyPolicyRule>
            {
                new CappedOccupancyPolicySummaryLengthRule(32),
                new CappedOccupancyPolicyNameLengthRule(80),
                new CappedOccupancyPolicyRequiredChannelRule("capped occupancy policy needs a value"),
                new CappedOccupancyPolicyRequiredCodeRule("capped occupancy policy needs this field"),
            });
    }
}
