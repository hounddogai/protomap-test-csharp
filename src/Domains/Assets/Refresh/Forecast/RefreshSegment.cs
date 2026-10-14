namespace Acme.Workforce.Assets.Refresh.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a refresh segment is in its life cycle.</summary>
public enum RefreshSegmentStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a refresh segment and its current status.</summary>
public sealed class RefreshSegment(string code, string title, string team, string salaryBand, double discount, int sequence, RefreshSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Team { get; set; } = team;
    public string SalaryBand { get; set; } = salaryBand;
    public double Discount { get; set; } = discount;
    public int Sequence { get; set; } = sequence;
    public RefreshSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a refresh segment in status proposed.</summary>
    public static RefreshSegment NewRefreshSegment(string code, string title)
    {
        return new RefreshSegment(code, title, "", "", 0.0, 0, RefreshSegmentStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RefreshSegmentStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RefreshSegmentStatus next)
    {
        switch (Status)
        {
            case RefreshSegmentStatus.Proposed:
                switch (next)
                {
                    case RefreshSegmentStatus.Rejected:
                    case RefreshSegmentStatus.Withdrawn:
                        return true;
                }
                return false;
            case RefreshSegmentStatus.Approved:
                switch (next)
                {
                    case RefreshSegmentStatus.Rejected:
                    case RefreshSegmentStatus.Withdrawn:
                        return true;
                }
                return false;
            case RefreshSegmentStatus.Rejected:
                return next == RefreshSegmentStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(RefreshSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"refresh segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"refresh segment"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round((double)Sequence + Discount, 2);
    }

    public RefreshSegment Copy()
    {
        return new RefreshSegment(Code, Title, Team, SalaryBand, Discount, Sequence, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
