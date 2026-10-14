namespace Acme.Workforce.Contractors.Rate.RateReview;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling rate assignment is in its life cycle.</summary>
public enum RollingRateAssignmentStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A rolling rate assignment. Its code is unique within a tenant.</summary>
public sealed class RollingRateAssignment(string code, string label, string source, int sequence, double rate, bool approved, RollingRateAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Source { get; set; } = source;
    public int Sequence { get; set; } = sequence;
    public double Rate { get; set; } = rate;
    public bool Approved { get; set; } = approved;
    public RollingRateAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a rolling rate assignment in status requested.</summary>
    public static RollingRateAssignment NewRollingRateAssignment(string code, string label)
    {
        return new RollingRateAssignment(code, label, "", 0, 0.0, false, RollingRateAssignmentStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingRateAssignmentStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingRateAssignmentStatus next)
    {
        switch (Status)
        {
            case RollingRateAssignmentStatus.Requested:
                switch (next)
                {
                    case RollingRateAssignmentStatus.Revoked:
                        return true;
                }
                return false;
            case RollingRateAssignmentStatus.Granted:
                return next == RollingRateAssignmentStatus.Revoked;
            case RollingRateAssignmentStatus.Revoked:
                switch (next)
                {
                    case RollingRateAssignmentStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingRateAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling rate assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double WeightedValue()
    {
        return Math.Round((double)Sequence - Rate, 2);
    }

    public RollingRateAssignment Copy()
    {
        return new RollingRateAssignment(Code, Label, Source, Sequence, Rate, Approved, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
