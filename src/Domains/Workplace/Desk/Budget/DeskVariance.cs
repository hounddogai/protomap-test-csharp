namespace Acme.Workforce.Workplace.Desk.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a desk variance is in its life cycle.</summary>
public enum DeskVarianceStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One desk variance, with what the planners need to act on it.</summary>
public sealed class DeskVariance(string code, string name, double ratio, double margin, int version, double hours, DeskVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Ratio { get; set; } = ratio;
    public double Margin { get; set; } = margin;
    public int Version { get; set; } = version;
    public double Hours { get; set; } = hours;
    public DeskVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a desk variance in status planned.</summary>
    public static DeskVariance NewDeskVariance(string code, string name)
    {
        return new DeskVariance(code, name, 0.0, 0.0, 0, 0.0, DeskVarianceStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeskVarianceStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeskVarianceStatus next)
    {
        switch (Status)
        {
            case DeskVarianceStatus.Planned:
                switch (next)
                {
                    case DeskVarianceStatus.InProgress:
                    case DeskVarianceStatus.Completed:
                        return true;
                }
                return false;
            case DeskVarianceStatus.Scheduled:
                switch (next)
                {
                    case DeskVarianceStatus.Completed:
                    case DeskVarianceStatus.Cancelled:
                        return true;
                }
                return false;
            case DeskVarianceStatus.InProgress:
                switch (next)
                {
                    case DeskVarianceStatus.Completed:
                        return true;
                }
                return false;
            case DeskVarianceStatus.Completed:
                return next == DeskVarianceStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(DeskVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"desk variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double AdjustedVolume()
    {
        return Math.Round(Margin - Hours, 2);
    }

    public DeskVariance Copy()
    {
        return new DeskVariance(Code, Name, Ratio, Margin, Version, Hours, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
