namespace Acme.Workforce.Talent.CareerPath.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback career path manifest is in its life cycle.</summary>
public enum FallbackCareerPathManifestStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A fallback career path manifest. Its code is unique within a tenant.</summary>
public sealed class FallbackCareerPathManifest(string code, string title, double cost, string grade, double floorValue, double hours, double fte, string costCenter, FallbackCareerPathManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Cost { get; set; } = cost;
    public string Grade { get; set; } = grade;
    public double FloorValue { get; set; } = floorValue;
    public double Hours { get; set; } = hours;
    public double Fte { get; set; } = fte;
    public string CostCenter { get; set; } = costCenter;
    public FallbackCareerPathManifestStatus Status { get; set; } = status;

    /// <summary>Starts a fallback career path manifest in status draft.</summary>
    public static FallbackCareerPathManifest NewFallbackCareerPathManifest(string code, string title)
    {
        return new FallbackCareerPathManifest(
            code,
            title,
            0.0,
            "",
            0.0,
            0.0,
            0.0,
            "",
            FallbackCareerPathManifestStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackCareerPathManifestStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackCareerPathManifestStatus next)
    {
        switch (Status)
        {
            case FallbackCareerPathManifestStatus.Draft:
                switch (next)
                {
                    case FallbackCareerPathManifestStatus.Suspended:
                    case FallbackCareerPathManifestStatus.Closed:
                        return true;
                }
                return false;
            case FallbackCareerPathManifestStatus.Pending:
                return next == FallbackCareerPathManifestStatus.Suspended || next == FallbackCareerPathManifestStatus.Closed;
            case FallbackCareerPathManifestStatus.Active:
                switch (next)
                {
                    case FallbackCareerPathManifestStatus.Suspended:
                    case FallbackCareerPathManifestStatus.Closed:
                        return true;
                }
                return false;
            case FallbackCareerPathManifestStatus.Suspended:
                switch (next)
                {
                    case FallbackCareerPathManifestStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackCareerPathManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback career path manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"fallback career path manifest"} {Code}";
    }

    public double WeightedExposure()
    {
        return Math.Round(FloorValue * Hours, 2);
    }

    public FallbackCareerPathManifest Copy()
    {
        return new FallbackCareerPathManifest(Code, Title, Cost, Grade, FloorValue, Hours, Fte, CostCenter, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
