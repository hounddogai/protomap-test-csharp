namespace Acme.Workforce.Absence.Holiday.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped holiday manifest is in its life cycle.</summary>
public enum CappedHolidayManifestStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One capped holiday manifest, with what the planners need to act on it.</summary>
public sealed class CappedHolidayManifest(string code, string label, double rate, bool pinned, string segment, CappedHolidayManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Rate { get; set; } = rate;
    public bool Pinned { get; set; } = pinned;
    public string Segment { get; set; } = segment;
    public CappedHolidayManifestStatus Status { get; set; } = status;

    /// <summary>Starts a capped holiday manifest in status draft.</summary>
    public static CappedHolidayManifest NewCappedHolidayManifest(string code, string label)
    {
        return new CappedHolidayManifest(code, label, 0.0, false, "", CappedHolidayManifestStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedHolidayManifestStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedHolidayManifestStatus next)
    {
        switch (Status)
        {
            case CappedHolidayManifestStatus.Draft:
                switch (next)
                {
                    case CappedHolidayManifestStatus.Pending:
                    case CappedHolidayManifestStatus.Suspended:
                        return true;
                }
                return false;
            case CappedHolidayManifestStatus.Pending:
                return next == CappedHolidayManifestStatus.Active || next == CappedHolidayManifestStatus.Suspended;
            case CappedHolidayManifestStatus.Active:
                return next == CappedHolidayManifestStatus.Suspended || next == CappedHolidayManifestStatus.Closed;
            case CappedHolidayManifestStatus.Suspended:
                switch (next)
                {
                    case CappedHolidayManifestStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedHolidayManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped holiday manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double MeasuredValue()
    {
        return Rate;
    }

    public CappedHolidayManifest Copy()
    {
        return new CappedHolidayManifest(Code, Label, Rate, Pinned, Segment, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
