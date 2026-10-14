namespace Acme.Workforce.Absence.Accrual.AccrualSlot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal accrual batch is in its life cycle.</summary>
public enum SeasonalAccrualBatchStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A seasonal accrual batch as the accrual team keeps it.</summary>
public sealed class SeasonalAccrualBatch(string code, string name, double budget, double margin, int leaveDays, string skill, string salaryBand, int sequence, int startsAt, string region, int attempts, SeasonalAccrualBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Budget { get; set; } = budget;
    public double Margin { get; set; } = margin;
    public int LeaveDays { get; set; } = leaveDays;
    public string Skill { get; set; } = skill;
    public string SalaryBand { get; set; } = salaryBand;
    public int Sequence { get; set; } = sequence;
    public int StartsAt { get; set; } = startsAt;
    public string Region { get; set; } = region;
    public int Attempts { get; set; } = attempts;
    public SeasonalAccrualBatchStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal accrual batch in status planned.</summary>
    public static SeasonalAccrualBatch NewSeasonalAccrualBatch(string code, string name)
    {
        return new SeasonalAccrualBatch(
            code,
            name,
            0.0,
            0.0,
            0,
            "",
            "",
            0,
            0,
            "",
            0,
            SeasonalAccrualBatchStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalAccrualBatchStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalAccrualBatchStatus next)
    {
        switch (Status)
        {
            case SeasonalAccrualBatchStatus.Planned:
                switch (next)
                {
                    case SeasonalAccrualBatchStatus.Cancelled:
                        return true;
                }
                return false;
            case SeasonalAccrualBatchStatus.Scheduled:
                switch (next)
                {
                    case SeasonalAccrualBatchStatus.Cancelled:
                        return true;
                }
                return false;
            case SeasonalAccrualBatchStatus.InProgress:
                return next == SeasonalAccrualBatchStatus.Completed || next == SeasonalAccrualBatchStatus.Cancelled;
            case SeasonalAccrualBatchStatus.Completed:
                switch (next)
                {
                    case SeasonalAccrualBatchStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalAccrualBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal accrual batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)StartsAt + (double)Attempts, 2);
    }

    public SeasonalAccrualBatch Copy()
    {
        return new SeasonalAccrualBatch(
            Code,
            Name,
            Budget,
            Margin,
            LeaveDays,
            Skill,
            SalaryBand,
            Sequence,
            StartsAt,
            Region,
            Attempts,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
