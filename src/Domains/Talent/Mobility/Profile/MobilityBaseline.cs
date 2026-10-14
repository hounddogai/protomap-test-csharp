namespace Acme.Workforce.Talent.Mobility.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a mobility baseline is in its life cycle.</summary>
public enum MobilityBaselineStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A mobility baseline. Its code is unique within a tenant.</summary>
public sealed class MobilityBaseline(string code, string title, int revision, double cost, string label, int expiresAt, MobilityBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Revision { get; set; } = revision;
    public double Cost { get; set; } = cost;
    public string Label { get; set; } = label;
    public int ExpiresAt { get; set; } = expiresAt;
    public MobilityBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a mobility baseline in status idle.</summary>
    public static MobilityBaseline NewMobilityBaseline(string code, string title)
    {
        return new MobilityBaseline(code, title, 0, 0.0, "", 0, MobilityBaselineStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MobilityBaselineStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MobilityBaselineStatus next)
    {
        switch (Status)
        {
            case MobilityBaselineStatus.Idle:
                return next == MobilityBaselineStatus.Ready || next == MobilityBaselineStatus.Draining;
            case MobilityBaselineStatus.Warming:
                return next == MobilityBaselineStatus.Ready || next == MobilityBaselineStatus.Stopped;
            case MobilityBaselineStatus.Ready:
                return next == MobilityBaselineStatus.Draining || next == MobilityBaselineStatus.Stopped;
            case MobilityBaselineStatus.Draining:
                return next == MobilityBaselineStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(MobilityBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"mobility baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{"mobility baseline"} {Code}";
    }

    public double NetValue()
    {
        return Math.Round((double)ExpiresAt + Cost, 2);
    }

    public MobilityBaseline Copy()
    {
        return new MobilityBaseline(Code, Title, Revision, Cost, Label, ExpiresAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
