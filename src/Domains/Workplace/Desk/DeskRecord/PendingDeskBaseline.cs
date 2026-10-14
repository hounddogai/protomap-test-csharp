namespace Acme.Workforce.Workplace.Desk.DeskRecord;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending desk baseline is in its life cycle.</summary>
public enum PendingDeskBaselineStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One pending desk baseline, with what the planners need to act on it.</summary>
public sealed class PendingDeskBaseline(string code, string name, int revision, double maximum, string reviewer, double budget, int windowDays, PendingDeskBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Revision { get; set; } = revision;
    public double Maximum { get; set; } = maximum;
    public string Reviewer { get; set; } = reviewer;
    public double Budget { get; set; } = budget;
    public int WindowDays { get; set; } = windowDays;
    public PendingDeskBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a pending desk baseline in status requested.</summary>
    public static PendingDeskBaseline NewPendingDeskBaseline(string code, string name)
    {
        return new PendingDeskBaseline(code, name, 0, 0.0, "", 0.0, 0, PendingDeskBaselineStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingDeskBaselineStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingDeskBaselineStatus next)
    {
        switch (Status)
        {
            case PendingDeskBaselineStatus.Requested:
                return next == PendingDeskBaselineStatus.Revoked;
            case PendingDeskBaselineStatus.Granted:
                return next == PendingDeskBaselineStatus.Revoked || next == PendingDeskBaselineStatus.Expired;
            case PendingDeskBaselineStatus.Revoked:
                switch (next)
                {
                    case PendingDeskBaselineStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingDeskBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending desk baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{"pending desk baseline"} {Code}";
    }

    public double WeightedLoad()
    {
        return Math.Round(Budget + (double)WindowDays, 2);
    }

    public PendingDeskBaseline Copy()
    {
        return new PendingDeskBaseline(Code, Name, Revision, Maximum, Reviewer, Budget, WindowDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
