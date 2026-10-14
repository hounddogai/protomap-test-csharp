namespace Acme.Workforce.Orgdesign.Reorg.StrictReorgBaseline;

/// <summary>A rule a pending reorg baseline must pass. Check returns its problems, or none.</summary>
public interface IPendingReorgBaselineRule
{
    List<string> Check(PendingReorgBaseline item);
}

public sealed record PendingReorgBaselineRequiredContactEmailRule(string Message) : IPendingReorgBaselineRule
{
    public List<string> Check(PendingReorgBaseline item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingReorgBaselineBadgeLengthRule(int MaxLength) : IPendingReorgBaselineRule
{
    public List<string> Check(PendingReorgBaseline item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingReorgBaselineCodePrefixRule(string Prefix) : IPendingReorgBaselineRule
{
    public List<string> Check(PendingReorgBaseline item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PendingReorgBaselineEndsAtRangeRule(int Min, int Max) : IPendingReorgBaselineRule
{
    public List<string> Check(PendingReorgBaseline item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PendingReorgBaselinePriorityRangeRule(int Min, int Max) : IPendingReorgBaselineRule
{
    public List<string> Check(PendingReorgBaseline item)
    {
        var valueValue = item.Priority;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"priority {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending reorg baseline.</summary>
public sealed record PendingReorgBaselineValidator(List<IPendingReorgBaselineRule> Rules)
{
    public List<string> Validate(PendingReorgBaseline item)
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

    public void RequireValid(PendingReorgBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingReorgBaseline> items)
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

/// <summary>Functions of pending reorg baseline rules.</summary>
public static class PendingReorgBaselineRules
{
    public static PendingReorgBaselineValidator DefaultPendingReorgBaselineValidator()
    {
        return new PendingReorgBaselineValidator(
            new List<IPendingReorgBaselineRule>
            {
                new PendingReorgBaselineRequiredContactEmailRule("pending reorg baseline needs an entry"),
                new PendingReorgBaselineBadgeLengthRule(3600),
                new PendingReorgBaselineCodePrefixRule("Q"),
                new PendingReorgBaselineEndsAtRangeRule(120, 3600),
                new PendingReorgBaselinePriorityRangeRule(32, 86400),
            });
    }
}
