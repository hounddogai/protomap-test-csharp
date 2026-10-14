namespace Acme.Workforce.Compensation.Adjustment.AdjustmentRule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adjustment manifest is in its life cycle.</summary>
public enum AdjustmentManifestStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A adjustment manifest. Its code is unique within a tenant.</summary>
public sealed class AdjustmentManifest(string code, string title, double score, int durationMinutes, string contactEmail, AdjustmentManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Score { get; set; } = score;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string ContactEmail { get; set; } = contactEmail;
    public AdjustmentManifestStatus Status { get; set; } = status;

    /// <summary>Starts a adjustment manifest in status proposed.</summary>
    public static AdjustmentManifest NewAdjustmentManifest(string code, string title)
    {
        return new AdjustmentManifest(code, title, 0.0, 0, "", AdjustmentManifestStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdjustmentManifestStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdjustmentManifestStatus next)
    {
        switch (Status)
        {
            case AdjustmentManifestStatus.Proposed:
                switch (next)
                {
                    case AdjustmentManifestStatus.Approved:
                        return true;
                }
                return false;
            case AdjustmentManifestStatus.Approved:
                switch (next)
                {
                    case AdjustmentManifestStatus.Rejected:
                        return true;
                }
                return false;
            case AdjustmentManifestStatus.Rejected:
                switch (next)
                {
                    case AdjustmentManifestStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdjustmentManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adjustment manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Score + (double)DurationMinutes, 2);
    }

    public AdjustmentManifest Copy()
    {
        return new AdjustmentManifest(Code, Title, Score, DurationMinutes, ContactEmail, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
