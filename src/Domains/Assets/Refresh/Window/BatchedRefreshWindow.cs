namespace Acme.Workforce.Assets.Refresh.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched refresh window is in its life cycle.</summary>
public enum BatchedRefreshWindowStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A batched refresh window as the refresh team keeps it.</summary>
public sealed class BatchedRefreshWindow(string code, string label, string salaryBand, int attempts, int tenureMonths, BatchedRefreshWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string SalaryBand { get; set; } = salaryBand;
    public int Attempts { get; set; } = attempts;
    public int TenureMonths { get; set; } = tenureMonths;
    public BatchedRefreshWindowStatus Status { get; set; } = status;

    /// <summary>Starts a batched refresh window in status proposed.</summary>
    public static BatchedRefreshWindow NewBatchedRefreshWindow(string code, string label)
    {
        return new BatchedRefreshWindow(code, label, "", 0, 0, BatchedRefreshWindowStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedRefreshWindowStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedRefreshWindowStatus next)
    {
        switch (Status)
        {
            case BatchedRefreshWindowStatus.Proposed:
                return next == BatchedRefreshWindowStatus.Rejected || next == BatchedRefreshWindowStatus.Withdrawn;
            case BatchedRefreshWindowStatus.Approved:
                switch (next)
                {
                    case BatchedRefreshWindowStatus.Rejected:
                    case BatchedRefreshWindowStatus.Withdrawn:
                        return true;
                }
                return false;
            case BatchedRefreshWindowStatus.Rejected:
                return next == BatchedRefreshWindowStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(BatchedRefreshWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched refresh window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double NetExposure()
    {
        return Math.Round((double)Attempts - (double)TenureMonths, 2);
    }

    public BatchedRefreshWindow Copy()
    {
        return new BatchedRefreshWindow(Code, Label, SalaryBand, Attempts, TenureMonths, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
