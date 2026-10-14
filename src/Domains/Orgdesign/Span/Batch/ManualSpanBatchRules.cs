namespace Acme.Workforce.Orgdesign.Span.Batch;

/// <summary>A rule a manual span batch must pass. Check returns its problems, or none.</summary>
public interface IManualSpanBatchRule
{
    List<string> Check(ManualSpanBatch item);
}

public sealed record ManualSpanBatchLeaveDaysRangeRule(int Min, int Max) : IManualSpanBatchRule
{
    public List<string> Check(ManualSpanBatch item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ManualSpanBatchRequiredChannelRule(string Message) : IManualSpanBatchRule
{
    public List<string> Check(ManualSpanBatch item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualSpanBatchCountryLengthRule(int MaxLength) : IManualSpanBatchRule
{
    public List<string> Check(ManualSpanBatch item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualSpanBatchCodePrefixRule(string Prefix) : IManualSpanBatchRule
{
    public List<string> Check(ManualSpanBatch item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ManualSpanBatchRequiredTitleRule(string Message) : IManualSpanBatchRule
{
    public List<string> Check(ManualSpanBatch item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual span batch.</summary>
public sealed record ManualSpanBatchValidator(List<IManualSpanBatchRule> Rules)
{
    public List<string> Validate(ManualSpanBatch item)
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

    public void RequireValid(ManualSpanBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualSpanBatch> items)
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

/// <summary>Functions of manual span batch rules.</summary>
public static class ManualSpanBatchRules
{
    public static ManualSpanBatchValidator DefaultManualSpanBatchValidator()
    {
        return new ManualSpanBatchValidator(
            new List<IManualSpanBatchRule>
            {
                new ManualSpanBatchLeaveDaysRangeRule(120, 120),
                new ManualSpanBatchRequiredChannelRule("manual span batch needs an entry"),
                new ManualSpanBatchCountryLengthRule(1000),
                new ManualSpanBatchCodePrefixRule("ACM"),
                new ManualSpanBatchRequiredTitleRule("manual span batch needs a value"),
            });
    }
}
