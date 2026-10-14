namespace Acme.Workforce.Workplace.AccessBadge.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a access badge reservation is in its life cycle.</summary>
public enum AccessBadgeReservationStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a access badge reservation and its current status.</summary>
public sealed class AccessBadgeReservation(string code, string label, bool visible, int batchSize, string notes, string managerId, string phone, string category, string source, int quantity, string employeeId, AccessBadgeReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Visible { get; set; } = visible;
    public int BatchSize { get; set; } = batchSize;
    public string Notes { get; set; } = notes;
    public string ManagerId { get; set; } = managerId;
    public string Phone { get; set; } = phone;
    public string Category { get; set; } = category;
    public string Source { get; set; } = source;
    public int Quantity { get; set; } = quantity;
    public string EmployeeId { get; set; } = employeeId;
    public AccessBadgeReservationStatus Status { get; set; } = status;

    /// <summary>Starts a access badge reservation in status idle.</summary>
    public static AccessBadgeReservation NewAccessBadgeReservation(string code, string label)
    {
        return new AccessBadgeReservation(
            code,
            label,
            false,
            0,
            "",
            "",
            "",
            "",
            "",
            0,
            "",
            AccessBadgeReservationStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccessBadgeReservationStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccessBadgeReservationStatus next)
    {
        switch (Status)
        {
            case AccessBadgeReservationStatus.Idle:
                switch (next)
                {
                    case AccessBadgeReservationStatus.Warming:
                    case AccessBadgeReservationStatus.Draining:
                        return true;
                }
                return false;
            case AccessBadgeReservationStatus.Warming:
                switch (next)
                {
                    case AccessBadgeReservationStatus.Stopped:
                        return true;
                }
                return false;
            case AccessBadgeReservationStatus.Ready:
                return next == AccessBadgeReservationStatus.Draining;
            case AccessBadgeReservationStatus.Draining:
                switch (next)
                {
                    case AccessBadgeReservationStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AccessBadgeReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"access badge reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)BatchSize - (double)Quantity, 2);
    }

    public AccessBadgeReservation Copy()
    {
        return new AccessBadgeReservation(
            Code,
            Label,
            Visible,
            BatchSize,
            Notes,
            ManagerId,
            Phone,
            Category,
            Source,
            Quantity,
            EmployeeId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
