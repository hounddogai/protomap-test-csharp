namespace Acme.Workforce.Orgdesign.Headcount.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a headcount window is in its life cycle.</summary>
public enum HeadcountWindowStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A headcount window. Its code is unique within a tenant.</summary>
public sealed class HeadcountWindow(string code, string label, bool archived, int leaveDays, string location, string source, int retries, HeadcountWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Archived { get; set; } = archived;
    public int LeaveDays { get; set; } = leaveDays;
    public string Location { get; set; } = location;
    public string Source { get; set; } = source;
    public int Retries { get; set; } = retries;
    public HeadcountWindowStatus Status { get; set; } = status;

    /// <summary>Starts a headcount window in status proposed.</summary>
    public static HeadcountWindow NewHeadcountWindow(string code, string label)
    {
        return new HeadcountWindow(code, label, false, 0, "", "", 0, HeadcountWindowStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HeadcountWindowStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HeadcountWindowStatus next)
    {
        switch (Status)
        {
            case HeadcountWindowStatus.Proposed:
                return next == HeadcountWindowStatus.Rejected || next == HeadcountWindowStatus.Withdrawn;
            case HeadcountWindowStatus.Approved:
                switch (next)
                {
                    case HeadcountWindowStatus.Rejected:
                    case HeadcountWindowStatus.Withdrawn:
                        return true;
                }
                return false;
            case HeadcountWindowStatus.Rejected:
                switch (next)
                {
                    case HeadcountWindowStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(HeadcountWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"headcount window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"headcount window"} {Code}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)LeaveDays - (double)Retries, 2);
    }

    public HeadcountWindow Copy()
    {
        return new HeadcountWindow(Code, Label, Archived, LeaveDays, Location, Source, Retries, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
