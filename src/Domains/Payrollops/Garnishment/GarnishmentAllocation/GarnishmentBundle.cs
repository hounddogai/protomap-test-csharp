namespace Acme.Workforce.Payrollops.Garnishment.GarnishmentAllocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a garnishment bundle is in its life cycle.</summary>
public enum GarnishmentBundleStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One garnishment bundle, with what the planners need to act on it.</summary>
public sealed class GarnishmentBundle(string code, string title, bool visible, string ownerId, int batchSize, string employeeId, int sequence, double weight, int tenureMonths, GarnishmentBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Visible { get; set; } = visible;
    public string OwnerId { get; set; } = ownerId;
    public int BatchSize { get; set; } = batchSize;
    public string EmployeeId { get; set; } = employeeId;
    public int Sequence { get; set; } = sequence;
    public double Weight { get; set; } = weight;
    public int TenureMonths { get; set; } = tenureMonths;
    public GarnishmentBundleStatus Status { get; set; } = status;

    /// <summary>Starts a garnishment bundle in status new.</summary>
    public static GarnishmentBundle NewGarnishmentBundle(string code, string title)
    {
        return new GarnishmentBundle(code, title, false, "", 0, "", 0, 0.0, 0, GarnishmentBundleStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GarnishmentBundleStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GarnishmentBundleStatus next)
    {
        switch (Status)
        {
            case GarnishmentBundleStatus.New:
                switch (next)
                {
                    case GarnishmentBundleStatus.Running:
                    case GarnishmentBundleStatus.Failed:
                        return true;
                }
                return false;
            case GarnishmentBundleStatus.Queued:
                return next == GarnishmentBundleStatus.Running;
            case GarnishmentBundleStatus.Running:
                return next == GarnishmentBundleStatus.Failed;
            case GarnishmentBundleStatus.Done:
                switch (next)
                {
                    case GarnishmentBundleStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GarnishmentBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"garnishment bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Weight - (double)TenureMonths, 2);
    }

    public GarnishmentBundle Copy()
    {
        return new GarnishmentBundle(
            Code,
            Title,
            Visible,
            OwnerId,
            BatchSize,
            EmployeeId,
            Sequence,
            Weight,
            TenureMonths,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
