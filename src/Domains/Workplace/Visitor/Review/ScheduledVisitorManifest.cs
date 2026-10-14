namespace Acme.Workforce.Workplace.Visitor.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled visitor manifest is in its life cycle.</summary>
public enum ScheduledVisitorManifestStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A scheduled visitor manifest. Its code is unique within a tenant.</summary>
public sealed class ScheduledVisitorManifest(string code, string title, int leaveDays, string channel, bool pinned, string phone, double ratio, string grade, string country, int updatedAt, double hours, ScheduledVisitorManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int LeaveDays { get; set; } = leaveDays;
    public string Channel { get; set; } = channel;
    public bool Pinned { get; set; } = pinned;
    public string Phone { get; set; } = phone;
    public double Ratio { get; set; } = ratio;
    public string Grade { get; set; } = grade;
    public string Country { get; set; } = country;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Hours { get; set; } = hours;
    public ScheduledVisitorManifestStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled visitor manifest in status idle.</summary>
    public static ScheduledVisitorManifest NewScheduledVisitorManifest(string code, string title)
    {
        return new ScheduledVisitorManifest(
            code,
            title,
            0,
            "",
            false,
            "",
            0.0,
            "",
            "",
            0,
            0.0,
            ScheduledVisitorManifestStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledVisitorManifestStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledVisitorManifestStatus next)
    {
        switch (Status)
        {
            case ScheduledVisitorManifestStatus.Idle:
                return next == ScheduledVisitorManifestStatus.Ready;
            case ScheduledVisitorManifestStatus.Warming:
                switch (next)
                {
                    case ScheduledVisitorManifestStatus.Ready:
                    case ScheduledVisitorManifestStatus.Stopped:
                        return true;
                }
                return false;
            case ScheduledVisitorManifestStatus.Ready:
                return next == ScheduledVisitorManifestStatus.Draining;
            case ScheduledVisitorManifestStatus.Draining:
                return next == ScheduledVisitorManifestStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(ScheduledVisitorManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled visitor manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"scheduled visitor manifest"} {Code}";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Hours - Ratio, 2);
    }

    public ScheduledVisitorManifest Copy()
    {
        return new ScheduledVisitorManifest(
            Code,
            Title,
            LeaveDays,
            Channel,
            Pinned,
            Phone,
            Ratio,
            Grade,
            Country,
            UpdatedAt,
            Hours,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
