namespace Acme.Workforce.Talent.CareerPath.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pending career path quota must pass. Check returns its problems, or none.</summary>
public interface IPendingCareerPathQuotaRule
{
    List<string> Check(PendingCareerPathQuota item);
}

public sealed record PendingCareerPathQuotaReviewerLengthRule(int MaxLength) : IPendingCareerPathQuotaRule
{
    public List<string> Check(PendingCareerPathQuota item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingCareerPathQuotaRequiredCodeRule(string Message) : IPendingCareerPathQuotaRule
{
    public List<string> Check(PendingCareerPathQuota item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingCareerPathQuotaAllowedReviewerRule(List<string> Allowed) : IPendingCareerPathQuotaRule
{
    public List<string> Check(PendingCareerPathQuota item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Reviewer)))
        {
            return new List<string> { $"reviewer {item.Reviewer} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record PendingCareerPathQuotaRequiredReviewerRule(string Message) : IPendingCareerPathQuotaRule
{
    public List<string> Check(PendingCareerPathQuota item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingCareerPathQuotaWeightRangeRule(double Min, double Max) : IPendingCareerPathQuotaRule
{
    public List<string> Check(PendingCareerPathQuota item)
    {
        var valueValue = item.Weight;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"weight {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"weight {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a pending career path quota.</summary>
public sealed record PendingCareerPathQuotaValidator(List<IPendingCareerPathQuotaRule> Rules)
{
    public List<string> Validate(PendingCareerPathQuota item)
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

    public void RequireValid(PendingCareerPathQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingCareerPathQuota> items)
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

/// <summary>Functions of pending career path quota rules.</summary>
public static class PendingCareerPathQuotaRules
{
    public static PendingCareerPathQuotaValidator DefaultPendingCareerPathQuotaValidator()
    {
        return new PendingCareerPathQuotaValidator(
            new List<IPendingCareerPathQuotaRule>
            {
                new PendingCareerPathQuotaReviewerLengthRule(120),
                new PendingCareerPathQuotaRequiredCodeRule("pending career path quota needs this field"),
                new PendingCareerPathQuotaAllowedReviewerRule(new List<string> { "adaptive", "rolling" }),
                new PendingCareerPathQuotaRequiredReviewerRule("pending career path quota needs an entry"),
                new PendingCareerPathQuotaWeightRangeRule(0.3, 2428.6),
            });
    }
}
