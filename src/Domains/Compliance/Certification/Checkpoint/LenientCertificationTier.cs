namespace Acme.Workforce.Compliance.Certification.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient certification tier is in its life cycle.</summary>
public enum LenientCertificationTierStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A lenient certification tier. Its code is unique within a tenant.</summary>
public sealed class LenientCertificationTier(string code, string name, int startsAt, string label, string team, string costCenter, string externalRef, string department, double score, int quantity, LenientCertificationTierStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int StartsAt { get; set; } = startsAt;
    public string Label { get; set; } = label;
    public string Team { get; set; } = team;
    public string CostCenter { get; set; } = costCenter;
    public string ExternalRef { get; set; } = externalRef;
    public string Department { get; set; } = department;
    public double Score { get; set; } = score;
    public int Quantity { get; set; } = quantity;
    public LenientCertificationTierStatus Status { get; set; } = status;

    /// <summary>Starts a lenient certification tier in status planned.</summary>
    public static LenientCertificationTier NewLenientCertificationTier(string code, string name)
    {
        return new LenientCertificationTier(
            code,
            name,
            0,
            "",
            "",
            "",
            "",
            "",
            0.0,
            0,
            LenientCertificationTierStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientCertificationTierStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientCertificationTierStatus next)
    {
        switch (Status)
        {
            case LenientCertificationTierStatus.Planned:
                return next == LenientCertificationTierStatus.Completed;
            case LenientCertificationTierStatus.Scheduled:
                return next == LenientCertificationTierStatus.Completed || next == LenientCertificationTierStatus.Cancelled;
            case LenientCertificationTierStatus.InProgress:
                switch (next)
                {
                    case LenientCertificationTierStatus.Completed:
                        return true;
                }
                return false;
            case LenientCertificationTierStatus.Completed:
                switch (next)
                {
                    case LenientCertificationTierStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientCertificationTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient certification tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Name}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)Quantity + (double)StartsAt, 2);
    }

    public LenientCertificationTier Copy()
    {
        return new LenientCertificationTier(
            Code,
            Name,
            StartsAt,
            Label,
            Team,
            CostCenter,
            ExternalRef,
            Department,
            Score,
            Quantity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
