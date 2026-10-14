namespace Acme.Workforce.Workplace.Desk.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a desk manifest is in its life cycle.</summary>
public enum DeskManifestStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A desk manifest. Its code is unique within a tenant.</summary>
public sealed class DeskManifest(string code, string name, string reviewer, int revision, string unit, DeskManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Reviewer { get; set; } = reviewer;
    public int Revision { get; set; } = revision;
    public string Unit { get; set; } = unit;
    public DeskManifestStatus Status { get; set; } = status;

    /// <summary>Starts a desk manifest in status requested.</summary>
    public static DeskManifest NewDeskManifest(string code, string name)
    {
        return new DeskManifest(code, name, "", 0, "", DeskManifestStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeskManifestStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeskManifestStatus next)
    {
        switch (Status)
        {
            case DeskManifestStatus.Requested:
                switch (next)
                {
                    case DeskManifestStatus.Granted:
                    case DeskManifestStatus.Expired:
                        return true;
                }
                return false;
            case DeskManifestStatus.Granted:
                switch (next)
                {
                    case DeskManifestStatus.Expired:
                        return true;
                }
                return false;
            case DeskManifestStatus.Revoked:
                switch (next)
                {
                    case DeskManifestStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DeskManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"desk manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{"desk manifest"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Revision;
    }

    public DeskManifest Copy()
    {
        return new DeskManifest(Code, Name, Reviewer, Revision, Unit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
