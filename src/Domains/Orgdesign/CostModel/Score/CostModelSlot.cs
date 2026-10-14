namespace Acme.Workforce.Orgdesign.CostModel.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a cost model slot is in its life cycle.</summary>
public enum CostModelSlotStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a cost model slot and its current status.</summary>
public sealed class CostModelSlot(string code, string title, int graceDays, string segment, string region, string notes, double threshold, int retries, string jobCode, int attempts, int startsAt, CostModelSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int GraceDays { get; set; } = graceDays;
    public string Segment { get; set; } = segment;
    public string Region { get; set; } = region;
    public string Notes { get; set; } = notes;
    public double Threshold { get; set; } = threshold;
    public int Retries { get; set; } = retries;
    public string JobCode { get; set; } = jobCode;
    public int Attempts { get; set; } = attempts;
    public int StartsAt { get; set; } = startsAt;
    public CostModelSlotStatus Status { get; set; } = status;

    /// <summary>Starts a cost model slot in status new.</summary>
    public static CostModelSlot NewCostModelSlot(string code, string title)
    {
        return new CostModelSlot(code, title, 0, "", "", "", 0.0, 0, "", 0, 0, CostModelSlotStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CostModelSlotStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CostModelSlotStatus next)
    {
        switch (Status)
        {
            case CostModelSlotStatus.New:
                return next == CostModelSlotStatus.Failed;
            case CostModelSlotStatus.Queued:
                return next == CostModelSlotStatus.Running || next == CostModelSlotStatus.Done;
            case CostModelSlotStatus.Running:
                return next == CostModelSlotStatus.Done || next == CostModelSlotStatus.Failed;
            case CostModelSlotStatus.Done:
                switch (next)
                {
                    case CostModelSlotStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CostModelSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"cost model slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)Attempts * (double)GraceDays, 2);
    }

    public CostModelSlot Copy()
    {
        return new CostModelSlot(
            Code,
            Title,
            GraceDays,
            Segment,
            Region,
            Notes,
            Threshold,
            Retries,
            JobCode,
            Attempts,
            StartsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
