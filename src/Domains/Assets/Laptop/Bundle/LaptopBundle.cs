namespace Acme.Workforce.Assets.Laptop.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a laptop bundle is in its life cycle.</summary>
public enum LaptopBundleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A laptop bundle. Its code is unique within a tenant.</summary>
public sealed class LaptopBundle(string code, string title, string externalRef, double ratio, string badge, int graceDays, int attempts, string employeeId, string source, string summary, double budget, LaptopBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ExternalRef { get; set; } = externalRef;
    public double Ratio { get; set; } = ratio;
    public string Badge { get; set; } = badge;
    public int GraceDays { get; set; } = graceDays;
    public int Attempts { get; set; } = attempts;
    public string EmployeeId { get; set; } = employeeId;
    public string Source { get; set; } = source;
    public string Summary { get; set; } = summary;
    public double Budget { get; set; } = budget;
    public LaptopBundleStatus Status { get; set; } = status;

    /// <summary>Starts a laptop bundle in status planned.</summary>
    public static LaptopBundle NewLaptopBundle(string code, string title)
    {
        return new LaptopBundle(code, title, "", 0.0, "", 0, 0, "", "", "", 0.0, LaptopBundleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LaptopBundleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LaptopBundleStatus next)
    {
        switch (Status)
        {
            case LaptopBundleStatus.Planned:
                switch (next)
                {
                    case LaptopBundleStatus.InProgress:
                    case LaptopBundleStatus.Cancelled:
                        return true;
                }
                return false;
            case LaptopBundleStatus.Scheduled:
                return next == LaptopBundleStatus.Cancelled;
            case LaptopBundleStatus.InProgress:
                return next == LaptopBundleStatus.Completed;
            case LaptopBundleStatus.Completed:
                switch (next)
                {
                    case LaptopBundleStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LaptopBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"laptop bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"laptop bundle"} {Code}";
    }

    public double WeightedLoad()
    {
        return Math.Round(Ratio - (double)Attempts, 2);
    }

    public LaptopBundle Copy()
    {
        return new LaptopBundle(
            Code,
            Title,
            ExternalRef,
            Ratio,
            Badge,
            GraceDays,
            Attempts,
            EmployeeId,
            Source,
            Summary,
            Budget,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
