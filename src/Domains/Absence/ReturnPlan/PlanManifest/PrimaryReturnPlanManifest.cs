namespace Acme.Workforce.Absence.ReturnPlan.PlanManifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary return plan manifest is in its life cycle.</summary>
public enum PrimaryReturnPlanManifestStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A primary return plan manifest as the return plan team keeps it.</summary>
public sealed class PrimaryReturnPlanManifest(string code, string title, int createdAt, string timezone, string jobCode, double hours, int expiresAt, PrimaryReturnPlanManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int CreatedAt { get; set; } = createdAt;
    public string Timezone { get; set; } = timezone;
    public string JobCode { get; set; } = jobCode;
    public double Hours { get; set; } = hours;
    public int ExpiresAt { get; set; } = expiresAt;
    public PrimaryReturnPlanManifestStatus Status { get; set; } = status;

    /// <summary>Starts a primary return plan manifest in status open.</summary>
    public static PrimaryReturnPlanManifest NewPrimaryReturnPlanManifest(string code, string title)
    {
        return new PrimaryReturnPlanManifest(code, title, 0, "", "", 0.0, 0, PrimaryReturnPlanManifestStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryReturnPlanManifestStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryReturnPlanManifestStatus next)
    {
        switch (Status)
        {
            case PrimaryReturnPlanManifestStatus.Open:
                return next == PrimaryReturnPlanManifestStatus.Resolved || next == PrimaryReturnPlanManifestStatus.Archived;
            case PrimaryReturnPlanManifestStatus.InReview:
                switch (next)
                {
                    case PrimaryReturnPlanManifestStatus.Reopened:
                    case PrimaryReturnPlanManifestStatus.Archived:
                        return true;
                }
                return false;
            case PrimaryReturnPlanManifestStatus.Resolved:
                return next == PrimaryReturnPlanManifestStatus.Archived;
            case PrimaryReturnPlanManifestStatus.Reopened:
                return next == PrimaryReturnPlanManifestStatus.Archived;
        }
        return false;
    }

    public void MoveTo(PrimaryReturnPlanManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary return plan manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"primary return plan manifest"} {Code}";
    }

    public double NetLoad()
    {
        return Math.Round(Hours + (double)CreatedAt, 2);
    }

    public PrimaryReturnPlanManifest Copy()
    {
        return new PrimaryReturnPlanManifest(Code, Title, CreatedAt, Timezone, JobCode, Hours, ExpiresAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
