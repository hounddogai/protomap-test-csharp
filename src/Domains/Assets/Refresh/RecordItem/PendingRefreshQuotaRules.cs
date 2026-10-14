namespace Acme.Workforce.Assets.Refresh.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pending refresh quota must pass. Check returns its problems, or none.</summary>
public interface IPendingRefreshQuotaRule
{
    List<string> Check(PendingRefreshQuota item);
}

public sealed record PendingRefreshQuotaTimezoneLengthRule(int MaxLength) : IPendingRefreshQuotaRule
{
    public List<string> Check(PendingRefreshQuota item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingRefreshQuotaBudgetRangeRule(double Min, double Max) : IPendingRefreshQuotaRule
{
    public List<string> Check(PendingRefreshQuota item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PendingRefreshQuotaRequiredTitleRule(string Message) : IPendingRefreshQuotaRule
{
    public List<string> Check(PendingRefreshQuota item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingRefreshQuotaAllowedTitleRule(List<string> Allowed) : IPendingRefreshQuotaRule
{
    public List<string> Check(PendingRefreshQuota item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is below minimum" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending refresh quota.</summary>
public sealed record PendingRefreshQuotaValidator(List<IPendingRefreshQuotaRule> Rules)
{
    public List<string> Validate(PendingRefreshQuota item)
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

    public void RequireValid(PendingRefreshQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingRefreshQuota> items)
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

/// <summary>Functions of pending refresh quota rules.</summary>
public static class PendingRefreshQuotaRules
{
    public static PendingRefreshQuotaValidator DefaultPendingRefreshQuotaValidator()
    {
        return new PendingRefreshQuotaValidator(
            new List<IPendingRefreshQuotaRule>
            {
                new PendingRefreshQuotaTimezoneLengthRule(1000),
                new PendingRefreshQuotaBudgetRangeRule(0.1, 2154.6),
                new PendingRefreshQuotaRequiredTitleRule("pending refresh quota needs this field"),
                new PendingRefreshQuotaAllowedTitleRule(new List<string> { "batched", "pending", "fallback" }),
            });
    }
}
