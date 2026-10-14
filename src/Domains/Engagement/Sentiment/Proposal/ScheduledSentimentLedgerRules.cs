namespace Acme.Workforce.Engagement.Sentiment.Proposal;

/// <summary>A rule a scheduled sentiment ledger must pass. Check returns its problems, or none.</summary>
public interface IScheduledSentimentLedgerRule
{
    List<string> Check(ScheduledSentimentLedger item);
}

public sealed record ScheduledSentimentLedgerChannelLengthRule(int MaxLength) : IScheduledSentimentLedgerRule
{
    public List<string> Check(ScheduledSentimentLedger item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledSentimentLedgerTenureMonthsRangeRule(int Min, int Max) : IScheduledSentimentLedgerRule
{
    public List<string> Check(ScheduledSentimentLedger item)
    {
        var valueValue = item.TenureMonths;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"tenure months {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledSentimentLedgerRequiredCodeRule(string Message) : IScheduledSentimentLedgerRule
{
    public List<string> Check(ScheduledSentimentLedger item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledSentimentLedgerRequiredDescriptionRule(string Message) : IScheduledSentimentLedgerRule
{
    public List<string> Check(ScheduledSentimentLedger item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledSentimentLedgerRequiredLabelRule(string Message) : IScheduledSentimentLedgerRule
{
    public List<string> Check(ScheduledSentimentLedger item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled sentiment ledger.</summary>
public sealed record ScheduledSentimentLedgerValidator(List<IScheduledSentimentLedgerRule> Rules)
{
    public List<string> Validate(ScheduledSentimentLedger item)
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

    public void RequireValid(ScheduledSentimentLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledSentimentLedger> items)
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

/// <summary>Functions of scheduled sentiment ledger rules.</summary>
public static class ScheduledSentimentLedgerRules
{
    public static ScheduledSentimentLedgerValidator DefaultScheduledSentimentLedgerValidator()
    {
        return new ScheduledSentimentLedgerValidator(
            new List<IScheduledSentimentLedgerRule>
            {
                new ScheduledSentimentLedgerChannelLengthRule(1000),
                new ScheduledSentimentLedgerTenureMonthsRangeRule(16, 120),
                new ScheduledSentimentLedgerRequiredCodeRule("scheduled sentiment ledger needs this field"),
                new ScheduledSentimentLedgerRequiredDescriptionRule("scheduled sentiment ledger needs this field"),
                new ScheduledSentimentLedgerRequiredLabelRule("scheduled sentiment ledger needs a value"),
            });
    }
}
