namespace Acme.Workforce.Compliance.Certification.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback certification bundle is in its life cycle.</summary>
public enum FallbackCertificationBundleStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A fallback certification bundle as the certification team keeps it.</summary>
public sealed class FallbackCertificationBundle(string code, string name, string grade, string team, string timezone, int quantity, int revision, FallbackCertificationBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Grade { get; set; } = grade;
    public string Team { get; set; } = team;
    public string Timezone { get; set; } = timezone;
    public int Quantity { get; set; } = quantity;
    public int Revision { get; set; } = revision;
    public FallbackCertificationBundleStatus Status { get; set; } = status;

    /// <summary>Starts a fallback certification bundle in status new.</summary>
    public static FallbackCertificationBundle NewFallbackCertificationBundle(string code, string name)
    {
        return new FallbackCertificationBundle(code, name, "", "", "", 0, 0, FallbackCertificationBundleStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackCertificationBundleStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackCertificationBundleStatus next)
    {
        switch (Status)
        {
            case FallbackCertificationBundleStatus.New:
                return next == FallbackCertificationBundleStatus.Running;
            case FallbackCertificationBundleStatus.Queued:
                switch (next)
                {
                    case FallbackCertificationBundleStatus.Running:
                        return true;
                }
                return false;
            case FallbackCertificationBundleStatus.Running:
                return next == FallbackCertificationBundleStatus.Done || next == FallbackCertificationBundleStatus.Failed;
            case FallbackCertificationBundleStatus.Done:
                switch (next)
                {
                    case FallbackCertificationBundleStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackCertificationBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback certification bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)Revision * (double)Quantity, 2);
    }

    public FallbackCertificationBundle Copy()
    {
        return new FallbackCertificationBundle(Code, Name, Grade, Team, Timezone, Quantity, Revision, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
