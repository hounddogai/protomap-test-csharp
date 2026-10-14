namespace Acme.Workforce.Absence.Accrual.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual accrual digest is in its life cycle.</summary>
public enum ManualAccrualDigestStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a manual accrual digest and its current status.</summary>
public sealed class ManualAccrualDigest(string code, string name, string location, string externalRef, int startsAt, ManualAccrualDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Location { get; set; } = location;
    public string ExternalRef { get; set; } = externalRef;
    public int StartsAt { get; set; } = startsAt;
    public ManualAccrualDigestStatus Status { get; set; } = status;

    /// <summary>Starts a manual accrual digest in status idle.</summary>
    public static ManualAccrualDigest NewManualAccrualDigest(string code, string name)
    {
        return new ManualAccrualDigest(code, name, "", "", 0, ManualAccrualDigestStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualAccrualDigestStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualAccrualDigestStatus next)
    {
        switch (Status)
        {
            case ManualAccrualDigestStatus.Idle:
                return next == ManualAccrualDigestStatus.Warming;
            case ManualAccrualDigestStatus.Warming:
                return next == ManualAccrualDigestStatus.Draining || next == ManualAccrualDigestStatus.Stopped;
            case ManualAccrualDigestStatus.Ready:
                return next == ManualAccrualDigestStatus.Draining || next == ManualAccrualDigestStatus.Stopped;
            case ManualAccrualDigestStatus.Draining:
                return next == ManualAccrualDigestStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(ManualAccrualDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual accrual digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"manual accrual digest"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)StartsAt;
    }

    public ManualAccrualDigest Copy()
    {
        return new ManualAccrualDigest(Code, Name, Location, ExternalRef, StartsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
