namespace Acme.Workforce.Payrollops.Payslip.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a payslip segment is in its life cycle.</summary>
public enum PayslipSegmentStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A payslip segment as the payslip team keeps it.</summary>
public sealed class PayslipSegment(string code, string title, string contactEmail, int updatedAt, double score, bool active, string region, int tenureMonths, PayslipSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ContactEmail { get; set; } = contactEmail;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Score { get; set; } = score;
    public bool Active { get; set; } = active;
    public string Region { get; set; } = region;
    public int TenureMonths { get; set; } = tenureMonths;
    public PayslipSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a payslip segment in status requested.</summary>
    public static PayslipSegment NewPayslipSegment(string code, string title)
    {
        return new PayslipSegment(code, title, "", 0, 0.0, false, "", 0, PayslipSegmentStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayslipSegmentStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayslipSegmentStatus next)
    {
        switch (Status)
        {
            case PayslipSegmentStatus.Requested:
                return next == PayslipSegmentStatus.Granted || next == PayslipSegmentStatus.Revoked;
            case PayslipSegmentStatus.Granted:
                return next == PayslipSegmentStatus.Revoked;
            case PayslipSegmentStatus.Revoked:
                switch (next)
                {
                    case PayslipSegmentStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PayslipSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"payslip segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"payslip segment"} {Code}";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)TenureMonths * Score, 2);
    }

    public PayslipSegment Copy()
    {
        return new PayslipSegment(Code, Title, ContactEmail, UpdatedAt, Score, Active, Region, TenureMonths, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
