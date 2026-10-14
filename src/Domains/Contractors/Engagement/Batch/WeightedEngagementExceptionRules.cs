namespace Acme.Workforce.Contractors.Engagement.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a weighted engagement exception must pass. Check returns its problems, or none.</summary>
public interface IWeightedEngagementExceptionRule
{
    List<string> Check(WeightedEngagementException item);
}

public sealed record WeightedEngagementExceptionAllowedCodeRule(List<string> Allowed) : IWeightedEngagementExceptionRule
{
    public List<string> Check(WeightedEngagementException item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record WeightedEngagementExceptionCodePrefixRule(string Prefix) : IWeightedEngagementExceptionRule
{
    public List<string> Check(WeightedEngagementException item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedEngagementExceptionRequiredTimezoneRule(string Message) : IWeightedEngagementExceptionRule
{
    public List<string> Check(WeightedEngagementException item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedEngagementExceptionArchivedGuardRule(double Limit) : IWeightedEngagementExceptionRule
{
    public List<string> Check(WeightedEngagementException item)
    {
        if (item.Archived && item.Discount > Limit)
        {
            return new List<string> { $"discount {item.Discount} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted engagement exception.</summary>
public sealed record WeightedEngagementExceptionValidator(List<IWeightedEngagementExceptionRule> Rules)
{
    public List<string> Validate(WeightedEngagementException item)
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

    public void RequireValid(WeightedEngagementException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedEngagementException> items)
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

/// <summary>Functions of weighted engagement exception rules.</summary>
public static class WeightedEngagementExceptionRules
{
    public static WeightedEngagementExceptionValidator DefaultWeightedEngagementExceptionValidator()
    {
        return new WeightedEngagementExceptionValidator(
            new List<IWeightedEngagementExceptionRule>
            {
                new WeightedEngagementExceptionAllowedCodeRule(new List<string> { "tiered", "seasonal" }),
                new WeightedEngagementExceptionCodePrefixRule("OPS"),
                new WeightedEngagementExceptionRequiredTimezoneRule("weighted engagement exception needs a value"),
                new WeightedEngagementExceptionArchivedGuardRule(0.4),
            });
    }
}
