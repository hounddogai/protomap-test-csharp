namespace Acme.Workforce.Compliance.Audit.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a audit estimate is in its life cycle.</summary>
public enum AuditEstimateStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A audit estimate as the audit team keeps it.</summary>
public sealed class AuditEstimate(string code, string name, int expiresAt, double rating, string checksum, bool visible, double cost, string employeeId, double score, string grade, double weight, AuditEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Rating { get; set; } = rating;
    public string Checksum { get; set; } = checksum;
    public bool Visible { get; set; } = visible;
    public double Cost { get; set; } = cost;
    public string EmployeeId { get; set; } = employeeId;
    public double Score { get; set; } = score;
    public string Grade { get; set; } = grade;
    public double Weight { get; set; } = weight;
    public AuditEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a audit estimate in status new.</summary>
    public static AuditEstimate NewAuditEstimate(string code, string name)
    {
        return new AuditEstimate(code, name, 0, 0.0, "", false, 0.0, "", 0.0, "", 0.0, AuditEstimateStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AuditEstimateStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AuditEstimateStatus next)
    {
        switch (Status)
        {
            case AuditEstimateStatus.New:
                switch (next)
                {
                    case AuditEstimateStatus.Done:
                        return true;
                }
                return false;
            case AuditEstimateStatus.Queued:
                switch (next)
                {
                    case AuditEstimateStatus.Running:
                        return true;
                }
                return false;
            case AuditEstimateStatus.Running:
                switch (next)
                {
                    case AuditEstimateStatus.Failed:
                        return true;
                }
                return false;
            case AuditEstimateStatus.Done:
                return next == AuditEstimateStatus.Failed;
        }
        return false;
    }

    public void MoveTo(AuditEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"audit estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double NetVolume()
    {
        return Math.Round(Cost + Rating, 2);
    }

    public AuditEstimate Copy()
    {
        return new AuditEstimate(
            Code,
            Name,
            ExpiresAt,
            Rating,
            Checksum,
            Visible,
            Cost,
            EmployeeId,
            Score,
            Grade,
            Weight,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
