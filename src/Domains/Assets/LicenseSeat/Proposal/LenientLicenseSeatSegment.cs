namespace Acme.Workforce.Assets.LicenseSeat.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient license seat segment is in its life cycle.</summary>
public enum LenientLicenseSeatSegmentStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A lenient license seat segment as the license seat team keeps it.</summary>
public sealed class LenientLicenseSeatSegment(string code, string label, string phone, double rating, string employeeId, int windowDays, double hours, LenientLicenseSeatSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Phone { get; set; } = phone;
    public double Rating { get; set; } = rating;
    public string EmployeeId { get; set; } = employeeId;
    public int WindowDays { get; set; } = windowDays;
    public double Hours { get; set; } = hours;
    public LenientLicenseSeatSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a lenient license seat segment in status draft.</summary>
    public static LenientLicenseSeatSegment NewLenientLicenseSeatSegment(string code, string label)
    {
        return new LenientLicenseSeatSegment(
            code,
            label,
            "",
            0.0,
            "",
            0,
            0.0,
            LenientLicenseSeatSegmentStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientLicenseSeatSegmentStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientLicenseSeatSegmentStatus next)
    {
        switch (Status)
        {
            case LenientLicenseSeatSegmentStatus.Draft:
                switch (next)
                {
                    case LenientLicenseSeatSegmentStatus.Suspended:
                    case LenientLicenseSeatSegmentStatus.Closed:
                        return true;
                }
                return false;
            case LenientLicenseSeatSegmentStatus.Pending:
                return next == LenientLicenseSeatSegmentStatus.Active || next == LenientLicenseSeatSegmentStatus.Closed;
            case LenientLicenseSeatSegmentStatus.Active:
                switch (next)
                {
                    case LenientLicenseSeatSegmentStatus.Suspended:
                    case LenientLicenseSeatSegmentStatus.Closed:
                        return true;
                }
                return false;
            case LenientLicenseSeatSegmentStatus.Suspended:
                switch (next)
                {
                    case LenientLicenseSeatSegmentStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientLicenseSeatSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient license seat segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)WindowDays - Hours, 2);
    }

    public LenientLicenseSeatSegment Copy()
    {
        return new LenientLicenseSeatSegment(Code, Label, Phone, Rating, EmployeeId, WindowDays, Hours, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
