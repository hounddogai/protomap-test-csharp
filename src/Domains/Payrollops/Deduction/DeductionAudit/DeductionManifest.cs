namespace Acme.Workforce.Payrollops.Deduction.DeductionAudit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a deduction manifest is in its life cycle.</summary>
public enum DeductionManifestStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One deduction manifest, with what the planners need to act on it.</summary>
public sealed class DeductionManifest(string code, string name, int windowDays, bool visible, string source, double score, DeductionManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int WindowDays { get; set; } = windowDays;
    public bool Visible { get; set; } = visible;
    public string Source { get; set; } = source;
    public double Score { get; set; } = score;
    public DeductionManifestStatus Status { get; set; } = status;

    /// <summary>Starts a deduction manifest in status open.</summary>
    public static DeductionManifest NewDeductionManifest(string code, string name)
    {
        return new DeductionManifest(code, name, 0, false, "", 0.0, DeductionManifestStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeductionManifestStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeductionManifestStatus next)
    {
        switch (Status)
        {
            case DeductionManifestStatus.Open:
                switch (next)
                {
                    case DeductionManifestStatus.InReview:
                        return true;
                }
                return false;
            case DeductionManifestStatus.InReview:
                return next == DeductionManifestStatus.Reopened;
            case DeductionManifestStatus.Resolved:
                return next == DeductionManifestStatus.Reopened;
            case DeductionManifestStatus.Reopened:
                return next == DeductionManifestStatus.Archived;
        }
        return false;
    }

    public void MoveTo(DeductionManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"deduction manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)WindowDays + Score, 2);
    }

    public DeductionManifest Copy()
    {
        return new DeductionManifest(Code, Name, WindowDays, Visible, Source, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
