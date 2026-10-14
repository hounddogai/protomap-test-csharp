namespace Acme.Workforce.Talent.SuccessionPlan.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a succession plan request is in its life cycle.</summary>
public enum SuccessionPlanRequestStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A succession plan request as the succession plan team keeps it.</summary>
public sealed class SuccessionPlanRequest(string code, string label, string costCenter, string externalRef, int batchSize, string notes, string location, double minimum, SuccessionPlanRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string CostCenter { get; set; } = costCenter;
    public string ExternalRef { get; set; } = externalRef;
    public int BatchSize { get; set; } = batchSize;
    public string Notes { get; set; } = notes;
    public string Location { get; set; } = location;
    public double Minimum { get; set; } = minimum;
    public SuccessionPlanRequestStatus Status { get; set; } = status;

    /// <summary>Starts a succession plan request in status new.</summary>
    public static SuccessionPlanRequest NewSuccessionPlanRequest(string code, string label)
    {
        return new SuccessionPlanRequest(code, label, "", "", 0, "", "", 0.0, SuccessionPlanRequestStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SuccessionPlanRequestStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SuccessionPlanRequestStatus next)
    {
        switch (Status)
        {
            case SuccessionPlanRequestStatus.New:
                return next == SuccessionPlanRequestStatus.Done || next == SuccessionPlanRequestStatus.Failed;
            case SuccessionPlanRequestStatus.Queued:
                return next == SuccessionPlanRequestStatus.Running;
            case SuccessionPlanRequestStatus.Running:
                return next == SuccessionPlanRequestStatus.Done;
            case SuccessionPlanRequestStatus.Done:
                return next == SuccessionPlanRequestStatus.Failed;
        }
        return false;
    }

    public void MoveTo(SuccessionPlanRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"succession plan request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)BatchSize - Minimum, 2);
    }

    public SuccessionPlanRequest Copy()
    {
        return new SuccessionPlanRequest(
            Code,
            Label,
            CostCenter,
            ExternalRef,
            BatchSize,
            Notes,
            Location,
            Minimum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
