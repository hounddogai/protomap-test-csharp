namespace Acme.Workforce.Compliance.Certification.CertificationSummary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a certification reservation is in its life cycle.</summary>
public enum CertificationReservationStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A certification reservation as the certification team keeps it.</summary>
public sealed class CertificationReservation(string code, string label, string employeeId, int leaveDays, double margin, string externalRef, int version, string location, int priority, int sequence, CertificationReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string EmployeeId { get; set; } = employeeId;
    public int LeaveDays { get; set; } = leaveDays;
    public double Margin { get; set; } = margin;
    public string ExternalRef { get; set; } = externalRef;
    public int Version { get; set; } = version;
    public string Location { get; set; } = location;
    public int Priority { get; set; } = priority;
    public int Sequence { get; set; } = sequence;
    public CertificationReservationStatus Status { get; set; } = status;

    /// <summary>Starts a certification reservation in status idle.</summary>
    public static CertificationReservation NewCertificationReservation(string code, string label)
    {
        return new CertificationReservation(
            code,
            label,
            "",
            0,
            0.0,
            "",
            0,
            "",
            0,
            0,
            CertificationReservationStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CertificationReservationStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CertificationReservationStatus next)
    {
        switch (Status)
        {
            case CertificationReservationStatus.Idle:
                switch (next)
                {
                    case CertificationReservationStatus.Ready:
                        return true;
                }
                return false;
            case CertificationReservationStatus.Warming:
                return next == CertificationReservationStatus.Draining || next == CertificationReservationStatus.Stopped;
            case CertificationReservationStatus.Ready:
                switch (next)
                {
                    case CertificationReservationStatus.Draining:
                        return true;
                }
                return false;
            case CertificationReservationStatus.Draining:
                switch (next)
                {
                    case CertificationReservationStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CertificationReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"certification reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double NetVolume()
    {
        return Math.Round((double)Version * (double)LeaveDays, 2);
    }

    public CertificationReservation Copy()
    {
        return new CertificationReservation(
            Code,
            Label,
            EmployeeId,
            LeaveDays,
            Margin,
            ExternalRef,
            Version,
            Location,
            Priority,
            Sequence,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
