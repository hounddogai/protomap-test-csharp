namespace Acme.Workforce.Workplace.Booking.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered booking assignment is in its life cycle.</summary>
public enum TieredBookingAssignmentStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A tiered booking assignment. Its code is unique within a tenant.</summary>
public sealed class TieredBookingAssignment(string code, string label, string ownerId, string employeeId, int quantity, string contactEmail, double threshold, string badge, int endsAt, TieredBookingAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string OwnerId { get; set; } = ownerId;
    public string EmployeeId { get; set; } = employeeId;
    public int Quantity { get; set; } = quantity;
    public string ContactEmail { get; set; } = contactEmail;
    public double Threshold { get; set; } = threshold;
    public string Badge { get; set; } = badge;
    public int EndsAt { get; set; } = endsAt;
    public TieredBookingAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a tiered booking assignment in status requested.</summary>
    public static TieredBookingAssignment NewTieredBookingAssignment(string code, string label)
    {
        return new TieredBookingAssignment(
            code,
            label,
            "",
            "",
            0,
            "",
            0.0,
            "",
            0,
            TieredBookingAssignmentStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredBookingAssignmentStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredBookingAssignmentStatus next)
    {
        switch (Status)
        {
            case TieredBookingAssignmentStatus.Requested:
                switch (next)
                {
                    case TieredBookingAssignmentStatus.Granted:
                        return true;
                }
                return false;
            case TieredBookingAssignmentStatus.Granted:
                return next == TieredBookingAssignmentStatus.Expired;
            case TieredBookingAssignmentStatus.Revoked:
                switch (next)
                {
                    case TieredBookingAssignmentStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredBookingAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered booking assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"tiered booking assignment"} {Code}";
    }

    public double WeightedExposure()
    {
        return Math.Round(Threshold - (double)EndsAt, 2);
    }

    public TieredBookingAssignment Copy()
    {
        return new TieredBookingAssignment(
            Code,
            Label,
            OwnerId,
            EmployeeId,
            Quantity,
            ContactEmail,
            Threshold,
            Badge,
            EndsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
