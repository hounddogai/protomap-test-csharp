namespace Acme.Workforce.Absence.Holiday.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a holiday entry is in its life cycle.</summary>
public enum HolidayEntryStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a holiday entry and its current status.</summary>
public sealed class HolidayEntry(string code, string title, int batchSize, double ratio, string timezone, int limit, string contactEmail, double ceiling, int updatedAt, HolidayEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int BatchSize { get; set; } = batchSize;
    public double Ratio { get; set; } = ratio;
    public string Timezone { get; set; } = timezone;
    public int Limit { get; set; } = limit;
    public string ContactEmail { get; set; } = contactEmail;
    public double Ceiling { get; set; } = ceiling;
    public int UpdatedAt { get; set; } = updatedAt;
    public HolidayEntryStatus Status { get; set; } = status;

    /// <summary>Starts a holiday entry in status open.</summary>
    public static HolidayEntry NewHolidayEntry(string code, string title)
    {
        return new HolidayEntry(code, title, 0, 0.0, "", 0, "", 0.0, 0, HolidayEntryStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HolidayEntryStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HolidayEntryStatus next)
    {
        switch (Status)
        {
            case HolidayEntryStatus.Open:
                switch (next)
                {
                    case HolidayEntryStatus.Reopened:
                        return true;
                }
                return false;
            case HolidayEntryStatus.InReview:
                return next == HolidayEntryStatus.Resolved;
            case HolidayEntryStatus.Resolved:
                return next == HolidayEntryStatus.Reopened || next == HolidayEntryStatus.Archived;
            case HolidayEntryStatus.Reopened:
                switch (next)
                {
                    case HolidayEntryStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(HolidayEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"holiday entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double NetValue()
    {
        return Math.Round((double)BatchSize * (double)Limit, 2);
    }

    public HolidayEntry Copy()
    {
        return new HolidayEntry(
            Code,
            Title,
            BatchSize,
            Ratio,
            Timezone,
            Limit,
            ContactEmail,
            Ceiling,
            UpdatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
