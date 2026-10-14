namespace Acme.Workforce.Payrollops.RetroPay.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional retro pay tier is in its life cycle.</summary>
public enum RegionalRetroPayTierStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a regional retro pay tier and its current status.</summary>
public sealed class RegionalRetroPayTier(string code, string label, string employeeId, bool visible, string category, double score, RegionalRetroPayTierStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string EmployeeId { get; set; } = employeeId;
    public bool Visible { get; set; } = visible;
    public string Category { get; set; } = category;
    public double Score { get; set; } = score;
    public RegionalRetroPayTierStatus Status { get; set; } = status;

    /// <summary>Starts a regional retro pay tier in status new.</summary>
    public static RegionalRetroPayTier NewRegionalRetroPayTier(string code, string label)
    {
        return new RegionalRetroPayTier(code, label, "", false, "", 0.0, RegionalRetroPayTierStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalRetroPayTierStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalRetroPayTierStatus next)
    {
        switch (Status)
        {
            case RegionalRetroPayTierStatus.New:
                return next == RegionalRetroPayTierStatus.Running;
            case RegionalRetroPayTierStatus.Queued:
                switch (next)
                {
                    case RegionalRetroPayTierStatus.Done:
                        return true;
                }
                return false;
            case RegionalRetroPayTierStatus.Running:
                return next == RegionalRetroPayTierStatus.Done || next == RegionalRetroPayTierStatus.Failed;
            case RegionalRetroPayTierStatus.Done:
                return next == RegionalRetroPayTierStatus.Failed;
        }
        return false;
    }

    public void MoveTo(RegionalRetroPayTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional retro pay tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public RegionalRetroPayTier Copy()
    {
        return new RegionalRetroPayTier(Code, Label, EmployeeId, Visible, Category, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
