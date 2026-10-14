namespace Acme.Workforce.Absence.ReturnPlan.PlanLedger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient return plan manifest is in its life cycle.</summary>
public enum LenientReturnPlanManifestStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One lenient return plan manifest, with what the planners need to act on it.</summary>
public sealed class LenientReturnPlanManifest(string code, string label, int tenureMonths, bool approved, int durationMinutes, double amount, LenientReturnPlanManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int TenureMonths { get; set; } = tenureMonths;
    public bool Approved { get; set; } = approved;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Amount { get; set; } = amount;
    public LenientReturnPlanManifestStatus Status { get; set; } = status;

    /// <summary>Starts a lenient return plan manifest in status created.</summary>
    public static LenientReturnPlanManifest NewLenientReturnPlanManifest(string code, string label)
    {
        return new LenientReturnPlanManifest(code, label, 0, false, 0, 0.0, LenientReturnPlanManifestStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientReturnPlanManifestStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientReturnPlanManifestStatus next)
    {
        switch (Status)
        {
            case LenientReturnPlanManifestStatus.Created:
                return next == LenientReturnPlanManifestStatus.Validated || next == LenientReturnPlanManifestStatus.Retired;
            case LenientReturnPlanManifestStatus.Validated:
                return next == LenientReturnPlanManifestStatus.Published || next == LenientReturnPlanManifestStatus.Retired;
            case LenientReturnPlanManifestStatus.Published:
                return next == LenientReturnPlanManifestStatus.Retired;
        }
        return false;
    }

    public void MoveTo(LenientReturnPlanManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient return plan manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"lenient return plan manifest"} {Code}";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)TenureMonths + (double)DurationMinutes, 2);
    }

    public LenientReturnPlanManifest Copy()
    {
        return new LenientReturnPlanManifest(Code, Label, TenureMonths, Approved, DurationMinutes, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
