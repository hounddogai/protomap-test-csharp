namespace Acme.Workforce.Compensation.MeritCycle.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a merit cycle entry is in its life cycle.</summary>
public enum MeritCycleEntryStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A merit cycle entry as the merit cycle team keeps it.</summary>
public sealed class MeritCycleEntry(string code, string title, double minimum, string managerId, int revision, double ratio, string badge, string salaryBand, MeritCycleEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Minimum { get; set; } = minimum;
    public string ManagerId { get; set; } = managerId;
    public int Revision { get; set; } = revision;
    public double Ratio { get; set; } = ratio;
    public string Badge { get; set; } = badge;
    public string SalaryBand { get; set; } = salaryBand;
    public MeritCycleEntryStatus Status { get; set; } = status;

    /// <summary>Starts a merit cycle entry in status planned.</summary>
    public static MeritCycleEntry NewMeritCycleEntry(string code, string title)
    {
        return new MeritCycleEntry(code, title, 0.0, "", 0, 0.0, "", "", MeritCycleEntryStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MeritCycleEntryStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MeritCycleEntryStatus next)
    {
        switch (Status)
        {
            case MeritCycleEntryStatus.Planned:
                switch (next)
                {
                    case MeritCycleEntryStatus.Scheduled:
                    case MeritCycleEntryStatus.InProgress:
                        return true;
                }
                return false;
            case MeritCycleEntryStatus.Scheduled:
                switch (next)
                {
                    case MeritCycleEntryStatus.Completed:
                    case MeritCycleEntryStatus.Cancelled:
                        return true;
                }
                return false;
            case MeritCycleEntryStatus.InProgress:
                switch (next)
                {
                    case MeritCycleEntryStatus.Completed:
                    case MeritCycleEntryStatus.Cancelled:
                        return true;
                }
                return false;
            case MeritCycleEntryStatus.Completed:
                return next == MeritCycleEntryStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(MeritCycleEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"merit cycle entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{"merit cycle entry"} {Code}";
    }

    public double WeightedVolume()
    {
        return Math.Round(Minimum + (double)Revision, 2);
    }

    public MeritCycleEntry Copy()
    {
        return new MeritCycleEntry(Code, Title, Minimum, ManagerId, Revision, Ratio, Badge, SalaryBand, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
