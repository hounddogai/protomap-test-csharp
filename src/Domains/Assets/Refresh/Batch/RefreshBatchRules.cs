namespace Acme.Workforce.Assets.Refresh.Batch;

/// <summary>A rule a refresh batch must pass. Check returns its problems, or none.</summary>
public interface IRefreshBatchRule
{
    List<string> Check(RefreshBatch item);
}

public sealed record RefreshBatchRequiredChannelRule(string Message) : IRefreshBatchRule
{
    public List<string> Check(RefreshBatch item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RefreshBatchRequiredCurrencyRule(string Message) : IRefreshBatchRule
{
    public List<string> Check(RefreshBatch item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RefreshBatchCodePrefixRule(string Prefix) : IRefreshBatchRule
{
    public List<string> Check(RefreshBatch item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshBatchRequiredCodeRule(string Message) : IRefreshBatchRule
{
    public List<string> Check(RefreshBatch item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RefreshBatchRequiredLabelRule(string Message) : IRefreshBatchRule
{
    public List<string> Check(RefreshBatch item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a refresh batch.</summary>
public sealed record RefreshBatchValidator(List<IRefreshBatchRule> Rules)
{
    public List<string> Validate(RefreshBatch item)
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

    public void RequireValid(RefreshBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RefreshBatch> items)
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

/// <summary>Functions of refresh batch rules.</summary>
public static class RefreshBatchRules
{
    public static RefreshBatchValidator DefaultRefreshBatchValidator()
    {
        return new RefreshBatchValidator(
            new List<IRefreshBatchRule>
            {
                new RefreshBatchRequiredChannelRule("refresh batch needs a value"),
                new RefreshBatchRequiredCurrencyRule("refresh batch needs this field"),
                new RefreshBatchCodePrefixRule("Q"),
                new RefreshBatchRequiredCodeRule("refresh batch needs a value"),
                new RefreshBatchRequiredLabelRule("refresh batch needs this field"),
            });
    }
}
