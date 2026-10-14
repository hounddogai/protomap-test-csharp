namespace Acme.Workforce.Compliance.Exception.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a exception entry is in its life cycle.</summary>
public enum ExceptionEntryStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A exception entry as the exception team keeps it.</summary>
public sealed class ExceptionEntry(string code, string title, string source, string skill, string region, double discount, string unit, double weight, string costCenter, string location, bool archived, ExceptionEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Source { get; set; } = source;
    public string Skill { get; set; } = skill;
    public string Region { get; set; } = region;
    public double Discount { get; set; } = discount;
    public string Unit { get; set; } = unit;
    public double Weight { get; set; } = weight;
    public string CostCenter { get; set; } = costCenter;
    public string Location { get; set; } = location;
    public bool Archived { get; set; } = archived;
    public ExceptionEntryStatus Status { get; set; } = status;

    /// <summary>Starts a exception entry in status new.</summary>
    public static ExceptionEntry NewExceptionEntry(string code, string title)
    {
        return new ExceptionEntry(code, title, "", "", "", 0.0, "", 0.0, "", "", false, ExceptionEntryStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ExceptionEntryStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ExceptionEntryStatus next)
    {
        switch (Status)
        {
            case ExceptionEntryStatus.New:
                switch (next)
                {
                    case ExceptionEntryStatus.Queued:
                    case ExceptionEntryStatus.Running:
                        return true;
                }
                return false;
            case ExceptionEntryStatus.Queued:
                return next == ExceptionEntryStatus.Done || next == ExceptionEntryStatus.Failed;
            case ExceptionEntryStatus.Running:
                return next == ExceptionEntryStatus.Done;
            case ExceptionEntryStatus.Done:
                return next == ExceptionEntryStatus.Failed;
        }
        return false;
    }

    public void MoveTo(ExceptionEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"exception entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double NetVolume()
    {
        return Math.Round(Discount + Weight, 2);
    }

    public ExceptionEntry Copy()
    {
        return new ExceptionEntry(
            Code,
            Title,
            Source,
            Skill,
            Region,
            Discount,
            Unit,
            Weight,
            CostCenter,
            Location,
            Archived,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
