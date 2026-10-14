namespace Acme.Workforce.Workplace.Booking.BookingVariance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy booking audit is in its life cycle.</summary>
public enum LegacyBookingAuditStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A legacy booking audit as the booking team keeps it.</summary>
public sealed class LegacyBookingAudit(string code, string title, string ownerId, int sequence, bool visible, string name, int quantity, int createdAt, string externalRef, int batchSize, LegacyBookingAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string OwnerId { get; set; } = ownerId;
    public int Sequence { get; set; } = sequence;
    public bool Visible { get; set; } = visible;
    public string Name { get; set; } = name;
    public int Quantity { get; set; } = quantity;
    public int CreatedAt { get; set; } = createdAt;
    public string ExternalRef { get; set; } = externalRef;
    public int BatchSize { get; set; } = batchSize;
    public LegacyBookingAuditStatus Status { get; set; } = status;

    /// <summary>Starts a legacy booking audit in status draft.</summary>
    public static LegacyBookingAudit NewLegacyBookingAudit(string code, string title)
    {
        return new LegacyBookingAudit(code, title, "", 0, false, "", 0, 0, "", 0, LegacyBookingAuditStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyBookingAuditStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyBookingAuditStatus next)
    {
        switch (Status)
        {
            case LegacyBookingAuditStatus.Draft:
                switch (next)
                {
                    case LegacyBookingAuditStatus.Pending:
                    case LegacyBookingAuditStatus.Active:
                        return true;
                }
                return false;
            case LegacyBookingAuditStatus.Pending:
                switch (next)
                {
                    case LegacyBookingAuditStatus.Active:
                    case LegacyBookingAuditStatus.Suspended:
                        return true;
                }
                return false;
            case LegacyBookingAuditStatus.Active:
                return next == LegacyBookingAuditStatus.Closed;
            case LegacyBookingAuditStatus.Suspended:
                switch (next)
                {
                    case LegacyBookingAuditStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyBookingAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy booking audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double NetExposure()
    {
        return Math.Round((double)BatchSize * (double)CreatedAt, 2);
    }

    public LegacyBookingAudit Copy()
    {
        return new LegacyBookingAudit(
            Code,
            Title,
            OwnerId,
            Sequence,
            Visible,
            Name,
            Quantity,
            CreatedAt,
            ExternalRef,
            BatchSize,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
