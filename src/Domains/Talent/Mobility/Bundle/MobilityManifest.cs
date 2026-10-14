namespace Acme.Workforce.Talent.Mobility.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a mobility manifest is in its life cycle.</summary>
public enum MobilityManifestStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One mobility manifest, with what the planners need to act on it.</summary>
public sealed class MobilityManifest(string code, string name, int durationMinutes, string managerId, string badge, MobilityManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string ManagerId { get; set; } = managerId;
    public string Badge { get; set; } = badge;
    public MobilityManifestStatus Status { get; set; } = status;

    /// <summary>Starts a mobility manifest in status created.</summary>
    public static MobilityManifest NewMobilityManifest(string code, string name)
    {
        return new MobilityManifest(code, name, 0, "", "", MobilityManifestStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MobilityManifestStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MobilityManifestStatus next)
    {
        switch (Status)
        {
            case MobilityManifestStatus.Created:
                return next == MobilityManifestStatus.Retired;
            case MobilityManifestStatus.Validated:
                return next == MobilityManifestStatus.Published || next == MobilityManifestStatus.Retired;
            case MobilityManifestStatus.Published:
                return next == MobilityManifestStatus.Retired;
        }
        return false;
    }

    public void MoveTo(MobilityManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"mobility manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double MeasuredValue()
    {
        return (double)DurationMinutes;
    }

    public MobilityManifest Copy()
    {
        return new MobilityManifest(Code, Name, DurationMinutes, ManagerId, Badge, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
