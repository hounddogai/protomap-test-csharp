namespace Acme.Workforce.Compensation.MeritCycle.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped merit cycle digest is in its life cycle.</summary>
public enum CappedMeritCycleDigestStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A capped merit cycle digest. Its code is unique within a tenant.</summary>
public sealed class CappedMeritCycleDigest(string code, string title, string timezone, int attempts, int graceDays, double ratio, CappedMeritCycleDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Timezone { get; set; } = timezone;
    public int Attempts { get; set; } = attempts;
    public int GraceDays { get; set; } = graceDays;
    public double Ratio { get; set; } = ratio;
    public CappedMeritCycleDigestStatus Status { get; set; } = status;

    /// <summary>Starts a capped merit cycle digest in status requested.</summary>
    public static CappedMeritCycleDigest NewCappedMeritCycleDigest(string code, string title)
    {
        return new CappedMeritCycleDigest(code, title, "", 0, 0, 0.0, CappedMeritCycleDigestStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedMeritCycleDigestStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedMeritCycleDigestStatus next)
    {
        switch (Status)
        {
            case CappedMeritCycleDigestStatus.Requested:
                switch (next)
                {
                    case CappedMeritCycleDigestStatus.Granted:
                    case CappedMeritCycleDigestStatus.Revoked:
                        return true;
                }
                return false;
            case CappedMeritCycleDigestStatus.Granted:
                switch (next)
                {
                    case CappedMeritCycleDigestStatus.Revoked:
                        return true;
                }
                return false;
            case CappedMeritCycleDigestStatus.Revoked:
                switch (next)
                {
                    case CappedMeritCycleDigestStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedMeritCycleDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped merit cycle digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double NetTotal()
    {
        return Math.Round(Ratio - (double)GraceDays, 2);
    }

    public CappedMeritCycleDigest Copy()
    {
        return new CappedMeritCycleDigest(Code, Title, Timezone, Attempts, GraceDays, Ratio, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
