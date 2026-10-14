namespace Acme.Workforce.Assets.LicenseSeat.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback license seat window is in its life cycle.</summary>
public enum FallbackLicenseSeatWindowStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a fallback license seat window and its current status.</summary>
public sealed class FallbackLicenseSeatWindow(string code, string name, List<string> tags, string label, bool pinned, double margin, int limit, string salaryBand, FallbackLicenseSeatWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public List<string> Tags { get; set; } = tags;
    public string Label { get; set; } = label;
    public bool Pinned { get; set; } = pinned;
    public double Margin { get; set; } = margin;
    public int Limit { get; set; } = limit;
    public string SalaryBand { get; set; } = salaryBand;
    public FallbackLicenseSeatWindowStatus Status { get; set; } = status;

    /// <summary>Starts a fallback license seat window in status planned.</summary>
    public static FallbackLicenseSeatWindow NewFallbackLicenseSeatWindow(string code, string name)
    {
        return new FallbackLicenseSeatWindow(
            code,
            name,
            new List<string>(),
            "",
            false,
            0.0,
            0,
            "",
            FallbackLicenseSeatWindowStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackLicenseSeatWindowStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackLicenseSeatWindowStatus next)
    {
        switch (Status)
        {
            case FallbackLicenseSeatWindowStatus.Planned:
                switch (next)
                {
                    case FallbackLicenseSeatWindowStatus.InProgress:
                        return true;
                }
                return false;
            case FallbackLicenseSeatWindowStatus.Scheduled:
                return next == FallbackLicenseSeatWindowStatus.InProgress || next == FallbackLicenseSeatWindowStatus.Completed;
            case FallbackLicenseSeatWindowStatus.InProgress:
                return next == FallbackLicenseSeatWindowStatus.Completed;
            case FallbackLicenseSeatWindowStatus.Completed:
                return next == FallbackLicenseSeatWindowStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(FallbackLicenseSeatWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback license seat window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"fallback license seat window"} {Code}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double NetValue()
    {
        return Math.Round(Margin - (double)Limit, 2);
    }

    public FallbackLicenseSeatWindow Copy()
    {
        return new FallbackLicenseSeatWindow(Code, Name, Tags, Label, Pinned, Margin, Limit, SalaryBand, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
