namespace Acme.Workforce.Compliance.Exception.RollingExceptionGroup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled exception draft is in its life cycle.</summary>
public enum ScheduledExceptionDraftStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A scheduled exception draft as the exception team keeps it.</summary>
public sealed class ScheduledExceptionDraft(string code, string label, int capacity, string segment, double fte, int endsAt, string badge, bool archived, ScheduledExceptionDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Capacity { get; set; } = capacity;
    public string Segment { get; set; } = segment;
    public double Fte { get; set; } = fte;
    public int EndsAt { get; set; } = endsAt;
    public string Badge { get; set; } = badge;
    public bool Archived { get; set; } = archived;
    public ScheduledExceptionDraftStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled exception draft in status created.</summary>
    public static ScheduledExceptionDraft NewScheduledExceptionDraft(string code, string label)
    {
        return new ScheduledExceptionDraft(
            code,
            label,
            0,
            "",
            0.0,
            0,
            "",
            false,
            ScheduledExceptionDraftStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledExceptionDraftStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledExceptionDraftStatus next)
    {
        switch (Status)
        {
            case ScheduledExceptionDraftStatus.Created:
                switch (next)
                {
                    case ScheduledExceptionDraftStatus.Validated:
                    case ScheduledExceptionDraftStatus.Retired:
                        return true;
                }
                return false;
            case ScheduledExceptionDraftStatus.Validated:
                return next == ScheduledExceptionDraftStatus.Published;
            case ScheduledExceptionDraftStatus.Published:
                return next == ScheduledExceptionDraftStatus.Retired;
        }
        return false;
    }

    public void MoveTo(ScheduledExceptionDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled exception draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double WeightedValue()
    {
        return Math.Round(Fte * (double)Capacity, 2);
    }

    public ScheduledExceptionDraft Copy()
    {
        return new ScheduledExceptionDraft(Code, Label, Capacity, Segment, Fte, EndsAt, Badge, Archived, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
