namespace Acme.Workforce.Engagement.TownHall.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a town hall manifest is in its life cycle.</summary>
public enum TownHallManifestStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A town hall manifest as the town hall team keeps it.</summary>
public sealed class TownHallManifest(string code, string name, double threshold, bool archived, int leaveDays, string externalRef, double maximum, TownHallManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Threshold { get; set; } = threshold;
    public bool Archived { get; set; } = archived;
    public int LeaveDays { get; set; } = leaveDays;
    public string ExternalRef { get; set; } = externalRef;
    public double Maximum { get; set; } = maximum;
    public TownHallManifestStatus Status { get; set; } = status;

    /// <summary>Starts a town hall manifest in status requested.</summary>
    public static TownHallManifest NewTownHallManifest(string code, string name)
    {
        return new TownHallManifest(code, name, 0.0, false, 0, "", 0.0, TownHallManifestStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TownHallManifestStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TownHallManifestStatus next)
    {
        switch (Status)
        {
            case TownHallManifestStatus.Requested:
                return next == TownHallManifestStatus.Granted || next == TownHallManifestStatus.Expired;
            case TownHallManifestStatus.Granted:
                switch (next)
                {
                    case TownHallManifestStatus.Revoked:
                    case TownHallManifestStatus.Expired:
                        return true;
                }
                return false;
            case TownHallManifestStatus.Revoked:
                switch (next)
                {
                    case TownHallManifestStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TownHallManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"town hall manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double NetTotal()
    {
        return Math.Round(Threshold - Maximum, 2);
    }

    public TownHallManifest Copy()
    {
        return new TownHallManifest(Code, Name, Threshold, Archived, LeaveDays, ExternalRef, Maximum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
