namespace Acme.Workforce.Compensation.Adjustment.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional adjustment reservation is in its life cycle.</summary>
public enum RegionalAdjustmentReservationStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A regional adjustment reservation. Its code is unique within a tenant.</summary>
public sealed class RegionalAdjustmentReservation(string code, string title, string ownerId, Dictionary<string, string> attributes, string category, int quantity, string phone, int sequence, double discount, int expiresAt, int graceDays, RegionalAdjustmentReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string OwnerId { get; set; } = ownerId;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Category { get; set; } = category;
    public int Quantity { get; set; } = quantity;
    public string Phone { get; set; } = phone;
    public int Sequence { get; set; } = sequence;
    public double Discount { get; set; } = discount;
    public int ExpiresAt { get; set; } = expiresAt;
    public int GraceDays { get; set; } = graceDays;
    public RegionalAdjustmentReservationStatus Status { get; set; } = status;

    /// <summary>Starts a regional adjustment reservation in status idle.</summary>
    public static RegionalAdjustmentReservation NewRegionalAdjustmentReservation(string code, string title)
    {
        return new RegionalAdjustmentReservation(
            code,
            title,
            "",
            new Dictionary<string, string>(),
            "",
            0,
            "",
            0,
            0.0,
            0,
            0,
            RegionalAdjustmentReservationStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalAdjustmentReservationStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalAdjustmentReservationStatus next)
    {
        switch (Status)
        {
            case RegionalAdjustmentReservationStatus.Idle:
                return next == RegionalAdjustmentReservationStatus.Warming || next == RegionalAdjustmentReservationStatus.Draining;
            case RegionalAdjustmentReservationStatus.Warming:
                switch (next)
                {
                    case RegionalAdjustmentReservationStatus.Draining:
                        return true;
                }
                return false;
            case RegionalAdjustmentReservationStatus.Ready:
                return next == RegionalAdjustmentReservationStatus.Draining || next == RegionalAdjustmentReservationStatus.Stopped;
            case RegionalAdjustmentReservationStatus.Draining:
                return next == RegionalAdjustmentReservationStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(RegionalAdjustmentReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional adjustment reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)Quantity + (double)Sequence, 2);
    }

    public RegionalAdjustmentReservation Copy()
    {
        return new RegionalAdjustmentReservation(
            Code,
            Title,
            OwnerId,
            Attributes,
            Category,
            Quantity,
            Phone,
            Sequence,
            Discount,
            ExpiresAt,
            GraceDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
