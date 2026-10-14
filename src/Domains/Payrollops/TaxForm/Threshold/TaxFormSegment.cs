namespace Acme.Workforce.Payrollops.TaxForm.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tax form segment is in its life cycle.</summary>
public enum TaxFormSegmentStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A tax form segment. Its code is unique within a tenant.</summary>
public sealed class TaxFormSegment(string code, string name, int tenureMonths, string team, string segment, TaxFormSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Team { get; set; } = team;
    public string Segment { get; set; } = segment;
    public TaxFormSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a tax form segment in status requested.</summary>
    public static TaxFormSegment NewTaxFormSegment(string code, string name)
    {
        return new TaxFormSegment(code, name, 0, "", "", TaxFormSegmentStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TaxFormSegmentStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TaxFormSegmentStatus next)
    {
        switch (Status)
        {
            case TaxFormSegmentStatus.Requested:
                switch (next)
                {
                    case TaxFormSegmentStatus.Granted:
                    case TaxFormSegmentStatus.Revoked:
                        return true;
                }
                return false;
            case TaxFormSegmentStatus.Granted:
                switch (next)
                {
                    case TaxFormSegmentStatus.Revoked:
                        return true;
                }
                return false;
            case TaxFormSegmentStatus.Revoked:
                switch (next)
                {
                    case TaxFormSegmentStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TaxFormSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tax form segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double MeasuredValue()
    {
        return (double)TenureMonths;
    }

    public TaxFormSegment Copy()
    {
        return new TaxFormSegment(Code, Name, TenureMonths, Team, Segment, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
