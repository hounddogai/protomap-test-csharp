namespace Acme.Workforce.Assets.Assignment.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional assignment entry is in its life cycle.</summary>
public enum RegionalAssignmentEntryStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a regional assignment entry and its current status.</summary>
public sealed class RegionalAssignmentEntry(string code, string title, bool active, int graceDays, double minimum, int endsAt, RegionalAssignmentEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Active { get; set; } = active;
    public int GraceDays { get; set; } = graceDays;
    public double Minimum { get; set; } = minimum;
    public int EndsAt { get; set; } = endsAt;
    public RegionalAssignmentEntryStatus Status { get; set; } = status;

    /// <summary>Starts a regional assignment entry in status open.</summary>
    public static RegionalAssignmentEntry NewRegionalAssignmentEntry(string code, string title)
    {
        return new RegionalAssignmentEntry(code, title, false, 0, 0.0, 0, RegionalAssignmentEntryStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalAssignmentEntryStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalAssignmentEntryStatus next)
    {
        switch (Status)
        {
            case RegionalAssignmentEntryStatus.Open:
                switch (next)
                {
                    case RegionalAssignmentEntryStatus.Reopened:
                        return true;
                }
                return false;
            case RegionalAssignmentEntryStatus.InReview:
                switch (next)
                {
                    case RegionalAssignmentEntryStatus.Resolved:
                    case RegionalAssignmentEntryStatus.Archived:
                        return true;
                }
                return false;
            case RegionalAssignmentEntryStatus.Resolved:
                return next == RegionalAssignmentEntryStatus.Archived;
            case RegionalAssignmentEntryStatus.Reopened:
                switch (next)
                {
                    case RegionalAssignmentEntryStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalAssignmentEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional assignment entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"regional assignment entry"} {Code}";
    }

    public double NetTotal()
    {
        return Math.Round((double)EndsAt + Minimum, 2);
    }

    public RegionalAssignmentEntry Copy()
    {
        return new RegionalAssignmentEntry(Code, Title, Active, GraceDays, Minimum, EndsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
