namespace Acme.Workforce.Compensation.Adjustment.Mapping;

/// <summary>A rule a capped adjustment agreement must pass. Check returns its problems, or none.</summary>
public interface ICappedAdjustmentAgreementRule
{
    List<string> Check(CappedAdjustmentAgreement item);
}

public sealed record CappedAdjustmentAgreementLocationLengthRule(int MaxLength) : ICappedAdjustmentAgreementRule
{
    public List<string> Check(CappedAdjustmentAgreement item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedAdjustmentAgreementTagsLimitRule(int MaxCount, List<string> Banned) : ICappedAdjustmentAgreementRule
{
    public List<string> Check(CappedAdjustmentAgreement item)
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

public sealed record CappedAdjustmentAgreementBudgetRangeRule(double Min, double Max) : ICappedAdjustmentAgreementRule
{
    public List<string> Check(CappedAdjustmentAgreement item)
    {
        var valueValue = item.Budget;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"budget {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"budget {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CappedAdjustmentAgreementCostRangeRule(double Min, double Max) : ICappedAdjustmentAgreementRule
{
    public List<string> Check(CappedAdjustmentAgreement item)
    {
        var valueValue = item.Cost;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"cost {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"cost {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CappedAdjustmentAgreementRetriesRangeRule(int Min, int Max) : ICappedAdjustmentAgreementRule
{
    public List<string> Check(CappedAdjustmentAgreement item)
    {
        var valueValue = item.Retries;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"retries {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"retries {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CappedAdjustmentAgreementNameLengthRule(int MaxLength) : ICappedAdjustmentAgreementRule
{
    public List<string> Check(CappedAdjustmentAgreement item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped adjustment agreement.</summary>
public sealed record CappedAdjustmentAgreementValidator(List<ICappedAdjustmentAgreementRule> Rules)
{
    public List<string> Validate(CappedAdjustmentAgreement item)
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

    public void RequireValid(CappedAdjustmentAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedAdjustmentAgreement> items)
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

/// <summary>Functions of capped adjustment agreement rules.</summary>
public static class CappedAdjustmentAgreementRules
{
    public static CappedAdjustmentAgreementValidator DefaultCappedAdjustmentAgreementValidator()
    {
        return new CappedAdjustmentAgreementValidator(
            new List<ICappedAdjustmentAgreementRule>
            {
                new CappedAdjustmentAgreementLocationLengthRule(3600),
                new CappedAdjustmentAgreementTagsLimitRule(80, new List<string> { "default" }),
                new CappedAdjustmentAgreementBudgetRangeRule(8.2, 2862.8),
                new CappedAdjustmentAgreementCostRangeRule(6.8, 3539.3),
                new CappedAdjustmentAgreementRetriesRangeRule(80, 255),
                new CappedAdjustmentAgreementNameLengthRule(86400),
            });
    }
}
