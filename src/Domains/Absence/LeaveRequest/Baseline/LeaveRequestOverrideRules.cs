namespace Acme.Workforce.Absence.LeaveRequest.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a leave request override must pass. Check returns its problems, or none.</summary>
public interface ILeaveRequestOverrideRule
{
    List<string> Check(LeaveRequestOverride item);
}

public sealed record LeaveRequestOverrideCodeLengthRule(int MaxLength) : ILeaveRequestOverrideRule
{
    public List<string> Check(LeaveRequestOverride item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestOverrideGradeLengthRule(int MaxLength) : ILeaveRequestOverrideRule
{
    public List<string> Check(LeaveRequestOverride item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestOverrideRequiredChannelRule(string Message) : ILeaveRequestOverrideRule
{
    public List<string> Check(LeaveRequestOverride item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestOverrideCodePrefixRule(string Prefix) : ILeaveRequestOverrideRule
{
    public List<string> Check(LeaveRequestOverride item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestOverrideGraceDaysRangeRule(int Min, int Max) : ILeaveRequestOverrideRule
{
    public List<string> Check(LeaveRequestOverride item)
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

public sealed record LeaveRequestOverrideAllowedChannelRule(List<string> Allowed) : ILeaveRequestOverrideRule
{
    public List<string> Check(LeaveRequestOverride item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Channel)))
        {
            return new List<string> { $"channel {item.Channel} is missing" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a leave request override.</summary>
public sealed record LeaveRequestOverrideValidator(List<ILeaveRequestOverrideRule> Rules)
{
    public List<string> Validate(LeaveRequestOverride item)
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

    public void RequireValid(LeaveRequestOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LeaveRequestOverride> items)
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

/// <summary>Functions of leave request override rules.</summary>
public static class LeaveRequestOverrideRules
{
    public static LeaveRequestOverrideValidator DefaultLeaveRequestOverrideValidator()
    {
        return new LeaveRequestOverrideValidator(
            new List<ILeaveRequestOverrideRule>
            {
                new LeaveRequestOverrideCodeLengthRule(120),
                new LeaveRequestOverrideGradeLengthRule(255),
                new LeaveRequestOverrideRequiredChannelRule("leave request override needs a value"),
                new LeaveRequestOverrideCodePrefixRule("X"),
                new LeaveRequestOverrideGraceDaysRangeRule(32, 32),
                new LeaveRequestOverrideAllowedChannelRule(new List<string> { "seasonal", "default", "manual" }),
            });
    }
}
