namespace Acme.Workforce.Workplace.Booking.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped booking agreement is in its life cycle.</summary>
public enum CappedBookingAgreementStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a capped booking agreement and its current status.</summary>
public sealed class CappedBookingAgreement(string code, string name, string description, string employeeId, string externalRef, int leaveDays, string checksum, int sequence, double cost, int attempts, CappedBookingAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Description { get; set; } = description;
    public string EmployeeId { get; set; } = employeeId;
    public string ExternalRef { get; set; } = externalRef;
    public int LeaveDays { get; set; } = leaveDays;
    public string Checksum { get; set; } = checksum;
    public int Sequence { get; set; } = sequence;
    public double Cost { get; set; } = cost;
    public int Attempts { get; set; } = attempts;
    public CappedBookingAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a capped booking agreement in status requested.</summary>
    public static CappedBookingAgreement NewCappedBookingAgreement(string code, string name)
    {
        return new CappedBookingAgreement(
            code,
            name,
            "",
            "",
            "",
            0,
            "",
            0,
            0.0,
            0,
            CappedBookingAgreementStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedBookingAgreementStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedBookingAgreementStatus next)
    {
        switch (Status)
        {
            case CappedBookingAgreementStatus.Requested:
                return next == CappedBookingAgreementStatus.Granted || next == CappedBookingAgreementStatus.Expired;
            case CappedBookingAgreementStatus.Granted:
                switch (next)
                {
                    case CappedBookingAgreementStatus.Revoked:
                        return true;
                }
                return false;
            case CappedBookingAgreementStatus.Revoked:
                switch (next)
                {
                    case CappedBookingAgreementStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedBookingAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped booking agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Cost + (double)LeaveDays, 2);
    }

    public CappedBookingAgreement Copy()
    {
        return new CappedBookingAgreement(
            Code,
            Name,
            Description,
            EmployeeId,
            ExternalRef,
            LeaveDays,
            Checksum,
            Sequence,
            Cost,
            Attempts,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
