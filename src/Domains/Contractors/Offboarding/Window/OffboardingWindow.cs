namespace Acme.Workforce.Contractors.Offboarding.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a offboarding window is in its life cycle.</summary>
public enum OffboardingWindowStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a offboarding window and its current status.</summary>
public sealed class OffboardingWindow(string code, string title, string location, string salaryBand, string costCenter, int attempts, int leaveDays, OffboardingWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Location { get; set; } = location;
    public string SalaryBand { get; set; } = salaryBand;
    public string CostCenter { get; set; } = costCenter;
    public int Attempts { get; set; } = attempts;
    public int LeaveDays { get; set; } = leaveDays;
    public OffboardingWindowStatus Status { get; set; } = status;

    /// <summary>Starts a offboarding window in status planned.</summary>
    public static OffboardingWindow NewOffboardingWindow(string code, string title)
    {
        return new OffboardingWindow(code, title, "", "", "", 0, 0, OffboardingWindowStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OffboardingWindowStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OffboardingWindowStatus next)
    {
        switch (Status)
        {
            case OffboardingWindowStatus.Planned:
                return next == OffboardingWindowStatus.Scheduled || next == OffboardingWindowStatus.InProgress;
            case OffboardingWindowStatus.Scheduled:
                switch (next)
                {
                    case OffboardingWindowStatus.InProgress:
                    case OffboardingWindowStatus.Cancelled:
                        return true;
                }
                return false;
            case OffboardingWindowStatus.InProgress:
                switch (next)
                {
                    case OffboardingWindowStatus.Cancelled:
                        return true;
                }
                return false;
            case OffboardingWindowStatus.Completed:
                switch (next)
                {
                    case OffboardingWindowStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(OffboardingWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"offboarding window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)Attempts * (double)LeaveDays, 2);
    }

    public OffboardingWindow Copy()
    {
        return new OffboardingWindow(Code, Title, Location, SalaryBand, CostCenter, Attempts, LeaveDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
