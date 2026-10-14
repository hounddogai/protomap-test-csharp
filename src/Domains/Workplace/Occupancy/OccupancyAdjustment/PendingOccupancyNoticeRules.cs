namespace Acme.Workforce.Workplace.Occupancy.OccupancyAdjustment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pending occupancy notice must pass. Check returns its problems, or none.</summary>
public interface IPendingOccupancyNoticeRule
{
    List<string> Check(PendingOccupancyNotice item);
}

public sealed record PendingOccupancyNoticeRequiredCodeRule(string Message) : IPendingOccupancyNoticeRule
{
    public List<string> Check(PendingOccupancyNotice item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingOccupancyNoticeAllowedExternalRefRule(List<string> Allowed) : IPendingOccupancyNoticeRule
{
    public List<string> Check(PendingOccupancyNotice item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ExternalRef)))
        {
            return new List<string> { $"external ref {item.ExternalRef} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record PendingOccupancyNoticeTenureMonthsRangeRule(int Min, int Max) : IPendingOccupancyNoticeRule
{
    public List<string> Check(PendingOccupancyNotice item)
    {
        var valueValue = item.TenureMonths;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"tenure months {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PendingOccupancyNoticeAttemptsRangeRule(int Min, int Max) : IPendingOccupancyNoticeRule
{
    public List<string> Check(PendingOccupancyNotice item)
    {
        var valueValue = item.Attempts;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"attempts {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"attempts {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PendingOccupancyNoticeExternalRefLengthRule(int MaxLength) : IPendingOccupancyNoticeRule
{
    public List<string> Check(PendingOccupancyNotice item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingOccupancyNoticeCodeLengthRule(int MaxLength) : IPendingOccupancyNoticeRule
{
    public List<string> Check(PendingOccupancyNotice item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending occupancy notice.</summary>
public sealed record PendingOccupancyNoticeValidator(List<IPendingOccupancyNoticeRule> Rules)
{
    public List<string> Validate(PendingOccupancyNotice item)
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

    public void RequireValid(PendingOccupancyNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingOccupancyNotice> items)
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

/// <summary>Functions of pending occupancy notice rules.</summary>
public static class PendingOccupancyNoticeRules
{
    public static PendingOccupancyNoticeValidator DefaultPendingOccupancyNoticeValidator()
    {
        return new PendingOccupancyNoticeValidator(
            new List<IPendingOccupancyNoticeRule>
            {
                new PendingOccupancyNoticeRequiredCodeRule("pending occupancy notice needs this field"),
                new PendingOccupancyNoticeAllowedExternalRefRule(
                    new List<string> { "weighted", "lenient", "seasonal" }),
                new PendingOccupancyNoticeTenureMonthsRangeRule(255, 3600),
                new PendingOccupancyNoticeAttemptsRangeRule(64, 3600),
                new PendingOccupancyNoticeExternalRefLengthRule(80),
                new PendingOccupancyNoticeCodeLengthRule(64),
            });
    }
}
