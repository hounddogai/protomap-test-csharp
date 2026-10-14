namespace Acme.Workforce.Workplace.Booking.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal booking template is in its life cycle.</summary>
public enum SeasonalBookingTemplateStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A seasonal booking template. Its code is unique within a tenant.</summary>
public sealed class SeasonalBookingTemplate(string code, string title, double hours, string segment, int quantity, int tenureMonths, string badge, SeasonalBookingTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Hours { get; set; } = hours;
    public string Segment { get; set; } = segment;
    public int Quantity { get; set; } = quantity;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Badge { get; set; } = badge;
    public SeasonalBookingTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal booking template in status requested.</summary>
    public static SeasonalBookingTemplate NewSeasonalBookingTemplate(string code, string title)
    {
        return new SeasonalBookingTemplate(code, title, 0.0, "", 0, 0, "", SeasonalBookingTemplateStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalBookingTemplateStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalBookingTemplateStatus next)
    {
        switch (Status)
        {
            case SeasonalBookingTemplateStatus.Requested:
                return next == SeasonalBookingTemplateStatus.Granted || next == SeasonalBookingTemplateStatus.Expired;
            case SeasonalBookingTemplateStatus.Granted:
                return next == SeasonalBookingTemplateStatus.Expired;
            case SeasonalBookingTemplateStatus.Revoked:
                switch (next)
                {
                    case SeasonalBookingTemplateStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalBookingTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal booking template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"seasonal booking template"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)TenureMonths + Hours, 2);
    }

    public SeasonalBookingTemplate Copy()
    {
        return new SeasonalBookingTemplate(Code, Title, Hours, Segment, Quantity, TenureMonths, Badge, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
