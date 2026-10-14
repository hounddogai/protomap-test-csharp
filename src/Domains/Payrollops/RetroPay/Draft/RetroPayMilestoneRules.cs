namespace Acme.Workforce.Payrollops.RetroPay.Draft;

/// <summary>A rule a retro pay milestone must pass. Check returns its problems, or none.</summary>
public interface IRetroPayMilestoneRule
{
    List<string> Check(RetroPayMilestone item);
}

public sealed record RetroPayMilestoneWindowDaysRangeRule(int Min, int Max) : IRetroPayMilestoneRule
{
    public List<string> Check(RetroPayMilestone item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayMilestoneCodePrefixRule(string Prefix) : IRetroPayMilestoneRule
{
    public List<string> Check(RetroPayMilestone item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayMilestoneRequiredCodeRule(string Message) : IRetroPayMilestoneRule
{
    public List<string> Check(RetroPayMilestone item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RetroPayMilestoneChecksumLengthRule(int MaxLength) : IRetroPayMilestoneRule
{
    public List<string> Check(RetroPayMilestone item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayMilestoneRequiredChecksumRule(string Message) : IRetroPayMilestoneRule
{
    public List<string> Check(RetroPayMilestone item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a retro pay milestone.</summary>
public sealed record RetroPayMilestoneValidator(List<IRetroPayMilestoneRule> Rules)
{
    public List<string> Validate(RetroPayMilestone item)
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

    public void RequireValid(RetroPayMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RetroPayMilestone> items)
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

/// <summary>Functions of retro pay milestone rules.</summary>
public static class RetroPayMilestoneRules
{
    public static RetroPayMilestoneValidator DefaultRetroPayMilestoneValidator()
    {
        return new RetroPayMilestoneValidator(
            new List<IRetroPayMilestoneRule>
            {
                new RetroPayMilestoneWindowDaysRangeRule(16, 16),
                new RetroPayMilestoneCodePrefixRule("REF"),
                new RetroPayMilestoneRequiredCodeRule("retro pay milestone needs a value"),
                new RetroPayMilestoneChecksumLengthRule(86400),
                new RetroPayMilestoneRequiredChecksumRule("retro pay milestone needs this field"),
            });
    }
}
