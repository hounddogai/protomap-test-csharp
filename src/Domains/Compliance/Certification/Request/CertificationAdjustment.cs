namespace Acme.Workforce.Compliance.Certification.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a certification adjustment is in its life cycle.</summary>
public enum CertificationAdjustmentStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One certification adjustment, with what the planners need to act on it.</summary>
public sealed class CertificationAdjustment(string code, string label, double threshold, string skill, int limit, double floorValue, double rate, int durationMinutes, string title, CertificationAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Threshold { get; set; } = threshold;
    public string Skill { get; set; } = skill;
    public int Limit { get; set; } = limit;
    public double FloorValue { get; set; } = floorValue;
    public double Rate { get; set; } = rate;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Title { get; set; } = title;
    public CertificationAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a certification adjustment in status requested.</summary>
    public static CertificationAdjustment NewCertificationAdjustment(string code, string label)
    {
        return new CertificationAdjustment(
            code,
            label,
            0.0,
            "",
            0,
            0.0,
            0.0,
            0,
            "",
            CertificationAdjustmentStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CertificationAdjustmentStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CertificationAdjustmentStatus next)
    {
        switch (Status)
        {
            case CertificationAdjustmentStatus.Requested:
                return next == CertificationAdjustmentStatus.Granted || next == CertificationAdjustmentStatus.Revoked;
            case CertificationAdjustmentStatus.Granted:
                return next == CertificationAdjustmentStatus.Revoked;
            case CertificationAdjustmentStatus.Revoked:
                switch (next)
                {
                    case CertificationAdjustmentStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CertificationAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"certification adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double NetExposure()
    {
        return Math.Round(Threshold * (double)DurationMinutes, 2);
    }

    public CertificationAdjustment Copy()
    {
        return new CertificationAdjustment(
            Code,
            Label,
            Threshold,
            Skill,
            Limit,
            FloorValue,
            Rate,
            DurationMinutes,
            Title,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
