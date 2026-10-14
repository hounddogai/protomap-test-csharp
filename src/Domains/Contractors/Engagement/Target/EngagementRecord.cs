namespace Acme.Workforce.Contractors.Engagement.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a engagement record is in its life cycle.</summary>
public enum EngagementRecordStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A engagement record as the engagement team keeps it.</summary>
public sealed class EngagementRecord(string code, string title, string externalRef, double fte, string summary, string channel, string segment, int durationMinutes, EngagementRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ExternalRef { get; set; } = externalRef;
    public double Fte { get; set; } = fte;
    public string Summary { get; set; } = summary;
    public string Channel { get; set; } = channel;
    public string Segment { get; set; } = segment;
    public int DurationMinutes { get; set; } = durationMinutes;
    public EngagementRecordStatus Status { get; set; } = status;

    /// <summary>Starts a engagement record in status created.</summary>
    public static EngagementRecord NewEngagementRecord(string code, string title)
    {
        return new EngagementRecord(code, title, "", 0.0, "", "", "", 0, EngagementRecordStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EngagementRecordStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EngagementRecordStatus next)
    {
        switch (Status)
        {
            case EngagementRecordStatus.Created:
                return next == EngagementRecordStatus.Published || next == EngagementRecordStatus.Retired;
            case EngagementRecordStatus.Validated:
                switch (next)
                {
                    case EngagementRecordStatus.Published:
                    case EngagementRecordStatus.Retired:
                        return true;
                }
                return false;
            case EngagementRecordStatus.Published:
                switch (next)
                {
                    case EngagementRecordStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(EngagementRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"engagement record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Fte * (double)DurationMinutes, 2);
    }

    public EngagementRecord Copy()
    {
        return new EngagementRecord(
            Code,
            Title,
            ExternalRef,
            Fte,
            Summary,
            Channel,
            Segment,
            DurationMinutes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
