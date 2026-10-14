namespace Acme.Workforce.Compliance.Audit.AuditManifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped audit reservation is in its life cycle.</summary>
public enum CappedAuditReservationStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A capped audit reservation. Its code is unique within a tenant.</summary>
public sealed class CappedAuditReservation(string code, string name, string department, int batchSize, string channel, int capacity, double fte, int attempts, double weight, string title, double discount, CappedAuditReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Department { get; set; } = department;
    public int BatchSize { get; set; } = batchSize;
    public string Channel { get; set; } = channel;
    public int Capacity { get; set; } = capacity;
    public double Fte { get; set; } = fte;
    public int Attempts { get; set; } = attempts;
    public double Weight { get; set; } = weight;
    public string Title { get; set; } = title;
    public double Discount { get; set; } = discount;
    public CappedAuditReservationStatus Status { get; set; } = status;

    /// <summary>Starts a capped audit reservation in status proposed.</summary>
    public static CappedAuditReservation NewCappedAuditReservation(string code, string name)
    {
        return new CappedAuditReservation(
            code,
            name,
            "",
            0,
            "",
            0,
            0.0,
            0,
            0.0,
            "",
            0.0,
            CappedAuditReservationStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedAuditReservationStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedAuditReservationStatus next)
    {
        switch (Status)
        {
            case CappedAuditReservationStatus.Proposed:
                switch (next)
                {
                    case CappedAuditReservationStatus.Approved:
                    case CappedAuditReservationStatus.Rejected:
                        return true;
                }
                return false;
            case CappedAuditReservationStatus.Approved:
                return next == CappedAuditReservationStatus.Rejected || next == CappedAuditReservationStatus.Withdrawn;
            case CappedAuditReservationStatus.Rejected:
                return next == CappedAuditReservationStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(CappedAuditReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped audit reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"capped audit reservation"} {Code}";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)Attempts + (double)Capacity, 2);
    }

    public CappedAuditReservation Copy()
    {
        return new CappedAuditReservation(
            Code,
            Name,
            Department,
            BatchSize,
            Channel,
            Capacity,
            Fte,
            Attempts,
            Weight,
            Title,
            Discount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
