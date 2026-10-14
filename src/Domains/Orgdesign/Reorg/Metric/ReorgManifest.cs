namespace Acme.Workforce.Orgdesign.Reorg.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a reorg manifest is in its life cycle.</summary>
public enum ReorgManifestStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One reorg manifest, with what the planners need to act on it.</summary>
public sealed class ReorgManifest(string code, string title, int tenureMonths, string timezone, string jobCode, ReorgManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Timezone { get; set; } = timezone;
    public string JobCode { get; set; } = jobCode;
    public ReorgManifestStatus Status { get; set; } = status;

    /// <summary>Starts a reorg manifest in status idle.</summary>
    public static ReorgManifest NewReorgManifest(string code, string title)
    {
        return new ReorgManifest(code, title, 0, "", "", ReorgManifestStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReorgManifestStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReorgManifestStatus next)
    {
        switch (Status)
        {
            case ReorgManifestStatus.Idle:
                switch (next)
                {
                    case ReorgManifestStatus.Warming:
                    case ReorgManifestStatus.Draining:
                        return true;
                }
                return false;
            case ReorgManifestStatus.Warming:
                return next == ReorgManifestStatus.Stopped;
            case ReorgManifestStatus.Ready:
                switch (next)
                {
                    case ReorgManifestStatus.Draining:
                    case ReorgManifestStatus.Stopped:
                        return true;
                }
                return false;
            case ReorgManifestStatus.Draining:
                return next == ReorgManifestStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(ReorgManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"reorg manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double MeasuredValue()
    {
        return (double)TenureMonths;
    }

    public ReorgManifest Copy()
    {
        return new ReorgManifest(Code, Title, TenureMonths, Timezone, JobCode, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
