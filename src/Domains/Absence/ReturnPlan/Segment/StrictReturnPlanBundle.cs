namespace Acme.Workforce.Absence.ReturnPlan.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict return plan bundle is in its life cycle.</summary>
public enum StrictReturnPlanBundleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a strict return plan bundle and its current status.</summary>
public sealed class StrictReturnPlanBundle(string code, string name, int version, bool approved, string title, string salaryBand, double minimum, string source, double margin, string notes, StrictReturnPlanBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Version { get; set; } = version;
    public bool Approved { get; set; } = approved;
    public string Title { get; set; } = title;
    public string SalaryBand { get; set; } = salaryBand;
    public double Minimum { get; set; } = minimum;
    public string Source { get; set; } = source;
    public double Margin { get; set; } = margin;
    public string Notes { get; set; } = notes;
    public StrictReturnPlanBundleStatus Status { get; set; } = status;

    /// <summary>Starts a strict return plan bundle in status planned.</summary>
    public static StrictReturnPlanBundle NewStrictReturnPlanBundle(string code, string name)
    {
        return new StrictReturnPlanBundle(
            code,
            name,
            0,
            false,
            "",
            "",
            0.0,
            "",
            0.0,
            "",
            StrictReturnPlanBundleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictReturnPlanBundleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictReturnPlanBundleStatus next)
    {
        switch (Status)
        {
            case StrictReturnPlanBundleStatus.Planned:
                switch (next)
                {
                    case StrictReturnPlanBundleStatus.InProgress:
                    case StrictReturnPlanBundleStatus.Cancelled:
                        return true;
                }
                return false;
            case StrictReturnPlanBundleStatus.Scheduled:
                switch (next)
                {
                    case StrictReturnPlanBundleStatus.Completed:
                        return true;
                }
                return false;
            case StrictReturnPlanBundleStatus.InProgress:
                switch (next)
                {
                    case StrictReturnPlanBundleStatus.Completed:
                        return true;
                }
                return false;
            case StrictReturnPlanBundleStatus.Completed:
                switch (next)
                {
                    case StrictReturnPlanBundleStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictReturnPlanBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict return plan bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double AdjustedVolume()
    {
        return Math.Round(Margin + Minimum, 2);
    }

    public StrictReturnPlanBundle Copy()
    {
        return new StrictReturnPlanBundle(
            Code,
            Name,
            Version,
            Approved,
            Title,
            SalaryBand,
            Minimum,
            Source,
            Margin,
            Notes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
