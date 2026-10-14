namespace Acme.Workforce.Assets.LicenseSeat.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a license seat checkpoint is in its life cycle.</summary>
public enum LicenseSeatCheckpointStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One license seat checkpoint, with what the planners need to act on it.</summary>
public sealed class LicenseSeatCheckpoint(string code, string title, string department, string category, string locale, double threshold, string jobCode, double budget, int startsAt, int endsAt, LicenseSeatCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Department { get; set; } = department;
    public string Category { get; set; } = category;
    public string Locale { get; set; } = locale;
    public double Threshold { get; set; } = threshold;
    public string JobCode { get; set; } = jobCode;
    public double Budget { get; set; } = budget;
    public int StartsAt { get; set; } = startsAt;
    public int EndsAt { get; set; } = endsAt;
    public LicenseSeatCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a license seat checkpoint in status proposed.</summary>
    public static LicenseSeatCheckpoint NewLicenseSeatCheckpoint(string code, string title)
    {
        return new LicenseSeatCheckpoint(
            code,
            title,
            "",
            "",
            "",
            0.0,
            "",
            0.0,
            0,
            0,
            LicenseSeatCheckpointStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LicenseSeatCheckpointStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LicenseSeatCheckpointStatus next)
    {
        switch (Status)
        {
            case LicenseSeatCheckpointStatus.Proposed:
                return next == LicenseSeatCheckpointStatus.Approved;
            case LicenseSeatCheckpointStatus.Approved:
                switch (next)
                {
                    case LicenseSeatCheckpointStatus.Withdrawn:
                        return true;
                }
                return false;
            case LicenseSeatCheckpointStatus.Rejected:
                return next == LicenseSeatCheckpointStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(LicenseSeatCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"license seat checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double NetValue()
    {
        return Math.Round((double)EndsAt - (double)StartsAt, 2);
    }

    public LicenseSeatCheckpoint Copy()
    {
        return new LicenseSeatCheckpoint(
            Code,
            Title,
            Department,
            Category,
            Locale,
            Threshold,
            JobCode,
            Budget,
            StartsAt,
            EndsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
