namespace Acme.Workforce.Payrollops.Deduction.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a deduction segment is in its life cycle.</summary>
public enum DeductionSegmentStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A deduction segment. Its code is unique within a tenant.</summary>
public sealed class DeductionSegment(string code, string name, string location, double rate, double ratio, string externalRef, string reviewer, DeductionSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Location { get; set; } = location;
    public double Rate { get; set; } = rate;
    public double Ratio { get; set; } = ratio;
    public string ExternalRef { get; set; } = externalRef;
    public string Reviewer { get; set; } = reviewer;
    public DeductionSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a deduction segment in status requested.</summary>
    public static DeductionSegment NewDeductionSegment(string code, string name)
    {
        return new DeductionSegment(code, name, "", 0.0, 0.0, "", "", DeductionSegmentStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeductionSegmentStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeductionSegmentStatus next)
    {
        switch (Status)
        {
            case DeductionSegmentStatus.Requested:
                switch (next)
                {
                    case DeductionSegmentStatus.Granted:
                        return true;
                }
                return false;
            case DeductionSegmentStatus.Granted:
                return next == DeductionSegmentStatus.Revoked || next == DeductionSegmentStatus.Expired;
            case DeductionSegmentStatus.Revoked:
                switch (next)
                {
                    case DeductionSegmentStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DeductionSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"deduction segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Ratio - Rate, 2);
    }

    public DeductionSegment Copy()
    {
        return new DeductionSegment(Code, Name, Location, Rate, Ratio, ExternalRef, Reviewer, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
