namespace Acme.Workforce.Workplace.Booking.StrictBookingPlan;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pending booking plan must pass. Check returns its problems, or none.</summary>
public interface IPendingBookingPlanRule
{
    List<string> Check(PendingBookingPlan item);
}

public sealed record PendingBookingPlanSequenceRangeRule(int Min, int Max) : IPendingBookingPlanRule
{
    public List<string> Check(PendingBookingPlan item)
    {
        var valueValue = item.Sequence;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"sequence {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"sequence {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PendingBookingPlanCodePrefixRule(string Prefix) : IPendingBookingPlanRule
{
    public List<string> Check(PendingBookingPlan item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PendingBookingPlanAllowedCodeRule(List<string> Allowed) : IPendingBookingPlanRule
{
    public List<string> Check(PendingBookingPlan item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is stale" };
        }
        return new List<string>();
    }
}

public sealed record PendingBookingPlanCapacityRangeRule(int Min, int Max) : IPendingBookingPlanRule
{
    public List<string> Check(PendingBookingPlan item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PendingBookingPlanLimitRangeRule(int Min, int Max) : IPendingBookingPlanRule
{
    public List<string> Check(PendingBookingPlan item)
    {
        var valueValue = item.Limit;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"limit {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending booking plan.</summary>
public sealed record PendingBookingPlanValidator(List<IPendingBookingPlanRule> Rules)
{
    public List<string> Validate(PendingBookingPlan item)
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

    public void RequireValid(PendingBookingPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingBookingPlan> items)
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

/// <summary>Functions of pending booking plan rules.</summary>
public static class PendingBookingPlanRules
{
    public static PendingBookingPlanValidator DefaultPendingBookingPlanValidator()
    {
        return new PendingBookingPlanValidator(
            new List<IPendingBookingPlanRule>
            {
                new PendingBookingPlanSequenceRangeRule(80, 1000),
                new PendingBookingPlanCodePrefixRule("REF"),
                new PendingBookingPlanAllowedCodeRule(new List<string> { "strict", "scheduled" }),
                new PendingBookingPlanCapacityRangeRule(64, 1000),
                new PendingBookingPlanLimitRangeRule(80, 1000),
            });
    }
}
