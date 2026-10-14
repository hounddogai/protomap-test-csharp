namespace Acme.Workforce.Payrollops.PayRun.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient pay run request is in its life cycle.</summary>
public enum LenientPayRunRequestStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One lenient pay run request, with what the planners need to act on it.</summary>
public sealed class LenientPayRunRequest(string code, string name, int version, string channel, string employeeId, int expiresAt, LenientPayRunRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Version { get; set; } = version;
    public string Channel { get; set; } = channel;
    public string EmployeeId { get; set; } = employeeId;
    public int ExpiresAt { get; set; } = expiresAt;
    public LenientPayRunRequestStatus Status { get; set; } = status;

    /// <summary>Starts a lenient pay run request in status requested.</summary>
    public static LenientPayRunRequest NewLenientPayRunRequest(string code, string name)
    {
        return new LenientPayRunRequest(code, name, 0, "", "", 0, LenientPayRunRequestStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientPayRunRequestStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientPayRunRequestStatus next)
    {
        switch (Status)
        {
            case LenientPayRunRequestStatus.Requested:
                return next == LenientPayRunRequestStatus.Granted || next == LenientPayRunRequestStatus.Expired;
            case LenientPayRunRequestStatus.Granted:
                switch (next)
                {
                    case LenientPayRunRequestStatus.Revoked:
                        return true;
                }
                return false;
            case LenientPayRunRequestStatus.Revoked:
                switch (next)
                {
                    case LenientPayRunRequestStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientPayRunRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient pay run request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)Version - (double)ExpiresAt, 2);
    }

    public LenientPayRunRequest Copy()
    {
        return new LenientPayRunRequest(Code, Name, Version, Channel, EmployeeId, ExpiresAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
