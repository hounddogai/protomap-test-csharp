namespace Acme.Workforce.Contractors.Rate.RateProposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched rate baseline is in its life cycle.</summary>
public enum BatchedRateBaselineStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A batched rate baseline as the rate team keeps it.</summary>
public sealed class BatchedRateBaseline(string code, string name, double ceiling, double discount, int windowDays, string title, double score, string timezone, bool active, string notes, int sequence, BatchedRateBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Ceiling { get; set; } = ceiling;
    public double Discount { get; set; } = discount;
    public int WindowDays { get; set; } = windowDays;
    public string Title { get; set; } = title;
    public double Score { get; set; } = score;
    public string Timezone { get; set; } = timezone;
    public bool Active { get; set; } = active;
    public string Notes { get; set; } = notes;
    public int Sequence { get; set; } = sequence;
    public BatchedRateBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a batched rate baseline in status planned.</summary>
    public static BatchedRateBaseline NewBatchedRateBaseline(string code, string name)
    {
        return new BatchedRateBaseline(
            code,
            name,
            0.0,
            0.0,
            0,
            "",
            0.0,
            "",
            false,
            "",
            0,
            BatchedRateBaselineStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedRateBaselineStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedRateBaselineStatus next)
    {
        switch (Status)
        {
            case BatchedRateBaselineStatus.Planned:
                switch (next)
                {
                    case BatchedRateBaselineStatus.Cancelled:
                        return true;
                }
                return false;
            case BatchedRateBaselineStatus.Scheduled:
                return next == BatchedRateBaselineStatus.Completed;
            case BatchedRateBaselineStatus.InProgress:
                switch (next)
                {
                    case BatchedRateBaselineStatus.Completed:
                    case BatchedRateBaselineStatus.Cancelled:
                        return true;
                }
                return false;
            case BatchedRateBaselineStatus.Completed:
                return next == BatchedRateBaselineStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(BatchedRateBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched rate baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double NetVolume()
    {
        return Math.Round((double)WindowDays - (double)Sequence, 2);
    }

    public BatchedRateBaseline Copy()
    {
        return new BatchedRateBaseline(
            Code,
            Name,
            Ceiling,
            Discount,
            WindowDays,
            Title,
            Score,
            Timezone,
            Active,
            Notes,
            Sequence,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
