namespace Acme.Workforce.Contractors.Rate.RateProposal;

/// <summary>A rule a pending rate window must pass. Check returns its problems, or none.</summary>
public interface IPendingRateWindowRule
{
    List<string> Check(PendingRateWindow item);
}

public sealed record PendingRateWindowGraceDaysRangeRule(int Min, int Max) : IPendingRateWindowRule
{
    public List<string> Check(PendingRateWindow item)
    {
        var valueValue = item.GraceDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"grace days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"grace days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PendingRateWindowArchivedGuardRule(double Limit) : IPendingRateWindowRule
{
    public List<string> Check(PendingRateWindow item)
    {
        if (item.Archived && item.Score > Limit)
        {
            return new List<string> { $"score {item.Score} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record PendingRateWindowRequiredChannelRule(string Message) : IPendingRateWindowRule
{
    public List<string> Check(PendingRateWindow item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingRateWindowChannelLengthRule(int MaxLength) : IPendingRateWindowRule
{
    public List<string> Check(PendingRateWindow item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending rate window.</summary>
public sealed record PendingRateWindowValidator(List<IPendingRateWindowRule> Rules)
{
    public List<string> Validate(PendingRateWindow item)
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

    public void RequireValid(PendingRateWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingRateWindow> items)
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

/// <summary>Functions of pending rate window rules.</summary>
public static class PendingRateWindowRules
{
    public static PendingRateWindowValidator DefaultPendingRateWindowValidator()
    {
        return new PendingRateWindowValidator(
            new List<IPendingRateWindowRule>
            {
                new PendingRateWindowGraceDaysRangeRule(1000, 3600),
                new PendingRateWindowArchivedGuardRule(9.7),
                new PendingRateWindowRequiredChannelRule("pending rate window needs an entry"),
                new PendingRateWindowChannelLengthRule(86400),
            });
    }
}
