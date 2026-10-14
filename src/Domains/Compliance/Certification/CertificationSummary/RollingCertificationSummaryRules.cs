namespace Acme.Workforce.Compliance.Certification.CertificationSummary;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rolling certification summary must pass. Check returns its problems, or none.</summary>
public interface IRollingCertificationSummaryRule
{
    List<string> Check(RollingCertificationSummary item);
}

public sealed record RollingCertificationSummaryRequiredJobCodeRule(string Message) : IRollingCertificationSummaryRule
{
    public List<string> Check(RollingCertificationSummary item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingCertificationSummaryAllowedJobCodeRule(List<string> Allowed) : IRollingCertificationSummaryRule
{
    public List<string> Check(RollingCertificationSummary item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.JobCode)))
        {
            return new List<string> { $"job code {item.JobCode} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record RollingCertificationSummaryAttemptsRangeRule(int Min, int Max) : IRollingCertificationSummaryRule
{
    public List<string> Check(RollingCertificationSummary item)
    {
        var valueValue = item.Attempts;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"attempts {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RollingCertificationSummaryTagsLimitRule(int MaxCount, List<string> Banned) : IRollingCertificationSummaryRule
{
    public List<string> Check(RollingCertificationSummary item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

public sealed record RollingCertificationSummaryRateRangeRule(double Min, double Max) : IRollingCertificationSummaryRule
{
    public List<string> Check(RollingCertificationSummary item)
    {
        var valueValue = item.Rate;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rate {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rate {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a rolling certification summary.</summary>
public sealed record RollingCertificationSummaryValidator(List<IRollingCertificationSummaryRule> Rules)
{
    public List<string> Validate(RollingCertificationSummary item)
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

    public void RequireValid(RollingCertificationSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingCertificationSummary> items)
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

/// <summary>Functions of rolling certification summary rules.</summary>
public static class RollingCertificationSummaryRules
{
    public static RollingCertificationSummaryValidator DefaultRollingCertificationSummaryValidator()
    {
        return new RollingCertificationSummaryValidator(
            new List<IRollingCertificationSummaryRule>
            {
                new RollingCertificationSummaryRequiredJobCodeRule("rolling certification summary needs an entry"),
                new RollingCertificationSummaryAllowedJobCodeRule(
                    new List<string> { "primary", "primary", "strict" }),
                new RollingCertificationSummaryAttemptsRangeRule(16, 86400),
                new RollingCertificationSummaryTagsLimitRule(1000, new List<string>()),
                new RollingCertificationSummaryRateRangeRule(7.5, 630.8),
            });
    }
}
