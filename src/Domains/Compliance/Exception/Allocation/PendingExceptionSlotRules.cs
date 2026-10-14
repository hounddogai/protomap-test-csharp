namespace Acme.Workforce.Compliance.Exception.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pending exception slot must pass. Check returns its problems, or none.</summary>
public interface IPendingExceptionSlotRule
{
    List<string> Check(PendingExceptionSlot item);
}

public sealed record PendingExceptionSlotRequiredTeamRule(string Message) : IPendingExceptionSlotRule
{
    public List<string> Check(PendingExceptionSlot item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingExceptionSlotAllowedTitleRule(List<string> Allowed) : IPendingExceptionSlotRule
{
    public List<string> Check(PendingExceptionSlot item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is too long" };
        }
        return new List<string>();
    }
}

public sealed record PendingExceptionSlotCodePrefixRule(string Prefix) : IPendingExceptionSlotRule
{
    public List<string> Check(PendingExceptionSlot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PendingExceptionSlotTitleLengthRule(int MaxLength) : IPendingExceptionSlotRule
{
    public List<string> Check(PendingExceptionSlot item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingExceptionSlotLimitRangeRule(int Min, int Max) : IPendingExceptionSlotRule
{
    public List<string> Check(PendingExceptionSlot item)
    {
        var valueValue = item.Limit;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"limit {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PendingExceptionSlotCodeLengthRule(int MaxLength) : IPendingExceptionSlotRule
{
    public List<string> Check(PendingExceptionSlot item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending exception slot.</summary>
public sealed record PendingExceptionSlotValidator(List<IPendingExceptionSlotRule> Rules)
{
    public List<string> Validate(PendingExceptionSlot item)
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

    public void RequireValid(PendingExceptionSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingExceptionSlot> items)
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

/// <summary>Functions of pending exception slot rules.</summary>
public static class PendingExceptionSlotRules
{
    public static PendingExceptionSlotValidator DefaultPendingExceptionSlotValidator()
    {
        return new PendingExceptionSlotValidator(
            new List<IPendingExceptionSlotRule>
            {
                new PendingExceptionSlotRequiredTeamRule("pending exception slot needs an entry"),
                new PendingExceptionSlotAllowedTitleRule(new List<string> { "batched" }),
                new PendingExceptionSlotCodePrefixRule("Q"),
                new PendingExceptionSlotTitleLengthRule(120),
                new PendingExceptionSlotLimitRangeRule(255, 255),
                new PendingExceptionSlotCodeLengthRule(32),
            });
    }
}
