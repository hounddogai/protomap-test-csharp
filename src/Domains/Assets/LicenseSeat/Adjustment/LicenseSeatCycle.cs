namespace Acme.Workforce.Assets.LicenseSeat.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a license seat cycle is in its life cycle.</summary>
public enum LicenseSeatCycleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A license seat cycle. Its code is unique within a tenant.</summary>
public sealed class LicenseSeatCycle(string code, string title, double budget, int revision, string reviewer, string name, string timezone, string summary, string grade, LicenseSeatCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Budget { get; set; } = budget;
    public int Revision { get; set; } = revision;
    public string Reviewer { get; set; } = reviewer;
    public string Name { get; set; } = name;
    public string Timezone { get; set; } = timezone;
    public string Summary { get; set; } = summary;
    public string Grade { get; set; } = grade;
    public LicenseSeatCycleStatus Status { get; set; } = status;

    /// <summary>Starts a license seat cycle in status planned.</summary>
    public static LicenseSeatCycle NewLicenseSeatCycle(string code, string title)
    {
        return new LicenseSeatCycle(code, title, 0.0, 0, "", "", "", "", "", LicenseSeatCycleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LicenseSeatCycleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LicenseSeatCycleStatus next)
    {
        switch (Status)
        {
            case LicenseSeatCycleStatus.Planned:
                return next == LicenseSeatCycleStatus.InProgress || next == LicenseSeatCycleStatus.Completed;
            case LicenseSeatCycleStatus.Scheduled:
                return next == LicenseSeatCycleStatus.InProgress || next == LicenseSeatCycleStatus.Completed;
            case LicenseSeatCycleStatus.InProgress:
                return next == LicenseSeatCycleStatus.Cancelled;
            case LicenseSeatCycleStatus.Completed:
                switch (next)
                {
                    case LicenseSeatCycleStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LicenseSeatCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"license seat cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Budget - (double)Revision, 2);
    }

    public LicenseSeatCycle Copy()
    {
        return new LicenseSeatCycle(Code, Title, Budget, Revision, Reviewer, Name, Timezone, Summary, Grade, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
