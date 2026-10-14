namespace Acme.Workforce.Workplace.Desk.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a desk estimate is in its life cycle.</summary>
public enum DeskEstimateStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A desk estimate as the desk team keeps it.</summary>
public sealed class DeskEstimate(string code, string title, bool pinned, double rating, int startsAt, string skill, string ownerId, string notes, int durationMinutes, DeskEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Pinned { get; set; } = pinned;
    public double Rating { get; set; } = rating;
    public int StartsAt { get; set; } = startsAt;
    public string Skill { get; set; } = skill;
    public string OwnerId { get; set; } = ownerId;
    public string Notes { get; set; } = notes;
    public int DurationMinutes { get; set; } = durationMinutes;
    public DeskEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a desk estimate in status draft.</summary>
    public static DeskEstimate NewDeskEstimate(string code, string title)
    {
        return new DeskEstimate(code, title, false, 0.0, 0, "", "", "", 0, DeskEstimateStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeskEstimateStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeskEstimateStatus next)
    {
        switch (Status)
        {
            case DeskEstimateStatus.Draft:
                return next == DeskEstimateStatus.Pending || next == DeskEstimateStatus.Closed;
            case DeskEstimateStatus.Pending:
                return next == DeskEstimateStatus.Suspended;
            case DeskEstimateStatus.Active:
                return next == DeskEstimateStatus.Suspended || next == DeskEstimateStatus.Closed;
            case DeskEstimateStatus.Suspended:
                switch (next)
                {
                    case DeskEstimateStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DeskEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"desk estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"desk estimate"} {Code}";
    }

    public double WeightedTotal()
    {
        return Math.Round(Rating * (double)DurationMinutes, 2);
    }

    public DeskEstimate Copy()
    {
        return new DeskEstimate(
            Code,
            Title,
            Pinned,
            Rating,
            StartsAt,
            Skill,
            OwnerId,
            Notes,
            DurationMinutes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
