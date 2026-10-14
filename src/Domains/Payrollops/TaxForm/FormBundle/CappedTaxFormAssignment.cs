namespace Acme.Workforce.Payrollops.TaxForm.FormBundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped tax form assignment is in its life cycle.</summary>
public enum CappedTaxFormAssignmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One capped tax form assignment, with what the planners need to act on it.</summary>
public sealed class CappedTaxFormAssignment(string code, string title, string source, int expiresAt, double ratio, int endsAt, int sequence, string description, double minimum, string costCenter, string country, CappedTaxFormAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Source { get; set; } = source;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Ratio { get; set; } = ratio;
    public int EndsAt { get; set; } = endsAt;
    public int Sequence { get; set; } = sequence;
    public string Description { get; set; } = description;
    public double Minimum { get; set; } = minimum;
    public string CostCenter { get; set; } = costCenter;
    public string Country { get; set; } = country;
    public CappedTaxFormAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a capped tax form assignment in status idle.</summary>
    public static CappedTaxFormAssignment NewCappedTaxFormAssignment(string code, string title)
    {
        return new CappedTaxFormAssignment(
            code,
            title,
            "",
            0,
            0.0,
            0,
            0,
            "",
            0.0,
            "",
            "",
            CappedTaxFormAssignmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedTaxFormAssignmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedTaxFormAssignmentStatus next)
    {
        switch (Status)
        {
            case CappedTaxFormAssignmentStatus.Idle:
                return next == CappedTaxFormAssignmentStatus.Warming || next == CappedTaxFormAssignmentStatus.Stopped;
            case CappedTaxFormAssignmentStatus.Warming:
                switch (next)
                {
                    case CappedTaxFormAssignmentStatus.Stopped:
                        return true;
                }
                return false;
            case CappedTaxFormAssignmentStatus.Ready:
                return next == CappedTaxFormAssignmentStatus.Draining || next == CappedTaxFormAssignmentStatus.Stopped;
            case CappedTaxFormAssignmentStatus.Draining:
                switch (next)
                {
                    case CappedTaxFormAssignmentStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedTaxFormAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped tax form assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)Sequence + Minimum, 2);
    }

    public CappedTaxFormAssignment Copy()
    {
        return new CappedTaxFormAssignment(
            Code,
            Title,
            Source,
            ExpiresAt,
            Ratio,
            EndsAt,
            Sequence,
            Description,
            Minimum,
            CostCenter,
            Country,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
