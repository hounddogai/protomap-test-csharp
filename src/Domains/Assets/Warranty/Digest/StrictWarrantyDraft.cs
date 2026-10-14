namespace Acme.Workforce.Assets.Warranty.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict warranty draft is in its life cycle.</summary>
public enum StrictWarrantyDraftStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A strict warranty draft as the warranty team keeps it.</summary>
public sealed class StrictWarrantyDraft(string code, string name, string grade, int leaveDays, string source, bool visible, int graceDays, StrictWarrantyDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Grade { get; set; } = grade;
    public int LeaveDays { get; set; } = leaveDays;
    public string Source { get; set; } = source;
    public bool Visible { get; set; } = visible;
    public int GraceDays { get; set; } = graceDays;
    public StrictWarrantyDraftStatus Status { get; set; } = status;

    /// <summary>Starts a strict warranty draft in status new.</summary>
    public static StrictWarrantyDraft NewStrictWarrantyDraft(string code, string name)
    {
        return new StrictWarrantyDraft(code, name, "", 0, "", false, 0, StrictWarrantyDraftStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictWarrantyDraftStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictWarrantyDraftStatus next)
    {
        switch (Status)
        {
            case StrictWarrantyDraftStatus.New:
                return next == StrictWarrantyDraftStatus.Failed;
            case StrictWarrantyDraftStatus.Queued:
                switch (next)
                {
                    case StrictWarrantyDraftStatus.Done:
                    case StrictWarrantyDraftStatus.Failed:
                        return true;
                }
                return false;
            case StrictWarrantyDraftStatus.Running:
                return next == StrictWarrantyDraftStatus.Done || next == StrictWarrantyDraftStatus.Failed;
            case StrictWarrantyDraftStatus.Done:
                switch (next)
                {
                    case StrictWarrantyDraftStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictWarrantyDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict warranty draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double NetTotal()
    {
        return Math.Round((double)LeaveDays - (double)GraceDays, 2);
    }

    public StrictWarrantyDraft Copy()
    {
        return new StrictWarrantyDraft(Code, Name, Grade, LeaveDays, Source, Visible, GraceDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
