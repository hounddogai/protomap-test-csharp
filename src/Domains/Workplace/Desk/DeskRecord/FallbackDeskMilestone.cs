namespace Acme.Workforce.Workplace.Desk.DeskRecord;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback desk milestone is in its life cycle.</summary>
public enum FallbackDeskMilestoneStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A fallback desk milestone as the desk team keeps it.</summary>
public sealed class FallbackDeskMilestone(string code, string title, int endsAt, double ceiling, int durationMinutes, int tenureMonths, string notes, double fte, Dictionary<string, string> attributes, FallbackDeskMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int EndsAt { get; set; } = endsAt;
    public double Ceiling { get; set; } = ceiling;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Notes { get; set; } = notes;
    public double Fte { get; set; } = fte;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public FallbackDeskMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a fallback desk milestone in status created.</summary>
    public static FallbackDeskMilestone NewFallbackDeskMilestone(string code, string title)
    {
        return new FallbackDeskMilestone(
            code,
            title,
            0,
            0.0,
            0,
            0,
            "",
            0.0,
            new Dictionary<string, string>(),
            FallbackDeskMilestoneStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackDeskMilestoneStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackDeskMilestoneStatus next)
    {
        switch (Status)
        {
            case FallbackDeskMilestoneStatus.Created:
                return next == FallbackDeskMilestoneStatus.Validated || next == FallbackDeskMilestoneStatus.Retired;
            case FallbackDeskMilestoneStatus.Validated:
                return next == FallbackDeskMilestoneStatus.Published;
            case FallbackDeskMilestoneStatus.Published:
                switch (next)
                {
                    case FallbackDeskMilestoneStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackDeskMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback desk milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double EffectiveLoad()
    {
        return Math.Round(Ceiling + Fte, 2);
    }

    public FallbackDeskMilestone Copy()
    {
        return new FallbackDeskMilestone(
            Code,
            Title,
            EndsAt,
            Ceiling,
            DurationMinutes,
            TenureMonths,
            Notes,
            Fte,
            Attributes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
