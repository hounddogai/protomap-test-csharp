namespace Acme.Workforce.Assets.Refresh.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal refresh window is in its life cycle.</summary>
public enum SeasonalRefreshWindowStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A seasonal refresh window as the refresh team keeps it.</summary>
public sealed class SeasonalRefreshWindow(string code, string title, string salaryBand, double maximum, string summary, SeasonalRefreshWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string SalaryBand { get; set; } = salaryBand;
    public double Maximum { get; set; } = maximum;
    public string Summary { get; set; } = summary;
    public SeasonalRefreshWindowStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal refresh window in status proposed.</summary>
    public static SeasonalRefreshWindow NewSeasonalRefreshWindow(string code, string title)
    {
        return new SeasonalRefreshWindow(code, title, "", 0.0, "", SeasonalRefreshWindowStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalRefreshWindowStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalRefreshWindowStatus next)
    {
        switch (Status)
        {
            case SeasonalRefreshWindowStatus.Proposed:
                switch (next)
                {
                    case SeasonalRefreshWindowStatus.Rejected:
                    case SeasonalRefreshWindowStatus.Withdrawn:
                        return true;
                }
                return false;
            case SeasonalRefreshWindowStatus.Approved:
                return next == SeasonalRefreshWindowStatus.Rejected || next == SeasonalRefreshWindowStatus.Withdrawn;
            case SeasonalRefreshWindowStatus.Rejected:
                return next == SeasonalRefreshWindowStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(SeasonalRefreshWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal refresh window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double MeasuredValue()
    {
        return Maximum;
    }

    public SeasonalRefreshWindow Copy()
    {
        return new SeasonalRefreshWindow(Code, Title, SalaryBand, Maximum, Summary, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
