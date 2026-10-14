namespace Acme.Workforce.Absence.Holiday.BatchedHolidaySchedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a holiday draft is in its life cycle.</summary>
public enum HolidayDraftStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A holiday draft as the holiday team keeps it.</summary>
public sealed class HolidayDraft(string code, string title, bool visible, string contactEmail, int updatedAt, string description, HolidayDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Visible { get; set; } = visible;
    public string ContactEmail { get; set; } = contactEmail;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Description { get; set; } = description;
    public HolidayDraftStatus Status { get; set; } = status;

    /// <summary>Starts a holiday draft in status requested.</summary>
    public static HolidayDraft NewHolidayDraft(string code, string title)
    {
        return new HolidayDraft(code, title, false, "", 0, "", HolidayDraftStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HolidayDraftStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HolidayDraftStatus next)
    {
        switch (Status)
        {
            case HolidayDraftStatus.Requested:
                return next == HolidayDraftStatus.Revoked;
            case HolidayDraftStatus.Granted:
                switch (next)
                {
                    case HolidayDraftStatus.Revoked:
                        return true;
                }
                return false;
            case HolidayDraftStatus.Revoked:
                return next == HolidayDraftStatus.Expired;
        }
        return false;
    }

    public void MoveTo(HolidayDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"holiday draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double MeasuredValue()
    {
        return (double)UpdatedAt;
    }

    public HolidayDraft Copy()
    {
        return new HolidayDraft(Code, Title, Visible, ContactEmail, UpdatedAt, Description, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
