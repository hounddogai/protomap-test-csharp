namespace Acme.Workforce.Absence.LeaveRequest.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a leave request draft must pass. Check returns its problems, or none.</summary>
public interface ILeaveRequestDraftRule
{
    List<string> Check(LeaveRequestDraft item);
}

public sealed record LeaveRequestDraftRequiredTitleRule(string Message) : ILeaveRequestDraftRule
{
    public List<string> Check(LeaveRequestDraft item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestDraftRequiredCurrencyRule(string Message) : ILeaveRequestDraftRule
{
    public List<string> Check(LeaveRequestDraft item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestDraftRateRangeRule(double Min, double Max) : ILeaveRequestDraftRule
{
    public List<string> Check(LeaveRequestDraft item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestDraftChannelLengthRule(int MaxLength) : ILeaveRequestDraftRule
{
    public List<string> Check(LeaveRequestDraft item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestDraftAllowedChannelRule(List<string> Allowed) : ILeaveRequestDraftRule
{
    public List<string> Check(LeaveRequestDraft item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Channel)))
        {
            return new List<string> { $"channel {item.Channel} is stale" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestDraftRequiredChannelRule(string Message) : ILeaveRequestDraftRule
{
    public List<string> Check(LeaveRequestDraft item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a leave request draft.</summary>
public sealed record LeaveRequestDraftValidator(List<ILeaveRequestDraftRule> Rules)
{
    public List<string> Validate(LeaveRequestDraft item)
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

    public void RequireValid(LeaveRequestDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LeaveRequestDraft> items)
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

/// <summary>Functions of leave request draft rules.</summary>
public static class LeaveRequestDraftRules
{
    public static LeaveRequestDraftValidator DefaultLeaveRequestDraftValidator()
    {
        return new LeaveRequestDraftValidator(
            new List<ILeaveRequestDraftRule>
            {
                new LeaveRequestDraftRequiredTitleRule("leave request draft needs an entry"),
                new LeaveRequestDraftRequiredCurrencyRule("leave request draft needs a value"),
                new LeaveRequestDraftRateRangeRule(1.0, 2635.7),
                new LeaveRequestDraftChannelLengthRule(16),
                new LeaveRequestDraftAllowedChannelRule(new List<string> { "pending" }),
                new LeaveRequestDraftRequiredChannelRule("leave request draft needs a value"),
            });
    }
}
