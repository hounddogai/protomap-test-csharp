namespace Acme.Workforce.Assets.Warranty.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pending warranty quota must pass. Check returns its problems, or none.</summary>
public interface IPendingWarrantyQuotaRule
{
    List<string> Check(PendingWarrantyQuota item);
}

public sealed record PendingWarrantyQuotaAllowedManagerIdRule(List<string> Allowed) : IPendingWarrantyQuotaRule
{
    public List<string> Check(PendingWarrantyQuota item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ManagerId)))
        {
            return new List<string> { $"manager id {item.ManagerId} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record PendingWarrantyQuotaBatchSizeRangeRule(int Min, int Max) : IPendingWarrantyQuotaRule
{
    public List<string> Check(PendingWarrantyQuota item)
    {
        var valueValue = item.BatchSize;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"batch size {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PendingWarrantyQuotaScoreRangeRule(double Min, double Max) : IPendingWarrantyQuotaRule
{
    public List<string> Check(PendingWarrantyQuota item)
    {
        var valueValue = item.Score;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"score {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"score {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PendingWarrantyQuotaLabelLengthRule(int MaxLength) : IPendingWarrantyQuotaRule
{
    public List<string> Check(PendingWarrantyQuota item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingWarrantyQuotaManagerIdLengthRule(int MaxLength) : IPendingWarrantyQuotaRule
{
    public List<string> Check(PendingWarrantyQuota item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending warranty quota.</summary>
public sealed record PendingWarrantyQuotaValidator(List<IPendingWarrantyQuotaRule> Rules)
{
    public List<string> Validate(PendingWarrantyQuota item)
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

    public void RequireValid(PendingWarrantyQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingWarrantyQuota> items)
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

/// <summary>Functions of pending warranty quota rules.</summary>
public static class PendingWarrantyQuotaRules
{
    public static PendingWarrantyQuotaValidator DefaultPendingWarrantyQuotaValidator()
    {
        return new PendingWarrantyQuotaValidator(
            new List<IPendingWarrantyQuotaRule>
            {
                new PendingWarrantyQuotaAllowedManagerIdRule(new List<string> { "tiered", "regional" }),
                new PendingWarrantyQuotaBatchSizeRangeRule(32, 3600),
                new PendingWarrantyQuotaScoreRangeRule(2.6, 4693.2),
                new PendingWarrantyQuotaLabelLengthRule(3600),
                new PendingWarrantyQuotaManagerIdLengthRule(1000),
            });
    }
}
