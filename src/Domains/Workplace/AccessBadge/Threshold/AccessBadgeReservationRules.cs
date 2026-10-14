namespace Acme.Workforce.Workplace.AccessBadge.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a access badge reservation must pass. Check returns its problems, or none.</summary>
public interface IAccessBadgeReservationRule
{
    List<string> Check(AccessBadgeReservation item);
}

public sealed record AccessBadgeReservationBatchSizeRangeRule(int Min, int Max) : IAccessBadgeReservationRule
{
    public List<string> Check(AccessBadgeReservation item)
    {
        var valueValue = item.BatchSize;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"batch size {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeReservationLabelLengthRule(int MaxLength) : IAccessBadgeReservationRule
{
    public List<string> Check(AccessBadgeReservation item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeReservationQuantityRangeRule(int Min, int Max) : IAccessBadgeReservationRule
{
    public List<string> Check(AccessBadgeReservation item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AccessBadgeReservationAllowedNotesRule(List<string> Allowed) : IAccessBadgeReservationRule
{
    public List<string> Check(AccessBadgeReservation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Notes)))
        {
            return new List<string> { $"notes {item.Notes} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeReservationManagerIdLengthRule(int MaxLength) : IAccessBadgeReservationRule
{
    public List<string> Check(AccessBadgeReservation item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeReservationRequiredLabelRule(string Message) : IAccessBadgeReservationRule
{
    public List<string> Check(AccessBadgeReservation item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a access badge reservation.</summary>
public sealed record AccessBadgeReservationValidator(List<IAccessBadgeReservationRule> Rules)
{
    public List<string> Validate(AccessBadgeReservation item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            var found = rule.Check(item);
            if (found.Count == 0)
            {
                continue;
            }
            foreach (var problem in found)
            {
                problems.Add($"{item.Code}: {problem}");
            }
        }
        return problems;
    }

    public void RequireValid(AccessBadgeReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccessBadgeReservation> items)
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

/// <summary>Functions of access badge reservation rules.</summary>
public static class AccessBadgeReservationRules
{
    public static AccessBadgeReservationValidator DefaultAccessBadgeReservationValidator()
    {
        return new AccessBadgeReservationValidator(
            new List<IAccessBadgeReservationRule>
            {
                new AccessBadgeReservationBatchSizeRangeRule(16, 80),
                new AccessBadgeReservationLabelLengthRule(80),
                new AccessBadgeReservationQuantityRangeRule(64, 3600),
                new AccessBadgeReservationAllowedNotesRule(new List<string> { "legacy", "default" }),
                new AccessBadgeReservationManagerIdLengthRule(3600),
                new AccessBadgeReservationRequiredLabelRule("access badge reservation needs this field"),
            });
    }
}
