namespace Acme.Workforce.Orgdesign.CostModel.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal cost model allocation is in its life cycle.</summary>
public enum SeasonalCostModelAllocationStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A seasonal cost model allocation. Its code is unique within a tenant.</summary>
public sealed class SeasonalCostModelAllocation(string code, string label, int windowDays, string checksum, string source, string timezone, string managerId, int leaveDays, int batchSize, SeasonalCostModelAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int WindowDays { get; set; } = windowDays;
    public string Checksum { get; set; } = checksum;
    public string Source { get; set; } = source;
    public string Timezone { get; set; } = timezone;
    public string ManagerId { get; set; } = managerId;
    public int LeaveDays { get; set; } = leaveDays;
    public int BatchSize { get; set; } = batchSize;
    public SeasonalCostModelAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal cost model allocation in status open.</summary>
    public static SeasonalCostModelAllocation NewSeasonalCostModelAllocation(string code, string label)
    {
        return new SeasonalCostModelAllocation(
            code,
            label,
            0,
            "",
            "",
            "",
            "",
            0,
            0,
            SeasonalCostModelAllocationStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalCostModelAllocationStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalCostModelAllocationStatus next)
    {
        switch (Status)
        {
            case SeasonalCostModelAllocationStatus.Open:
                return next == SeasonalCostModelAllocationStatus.Resolved || next == SeasonalCostModelAllocationStatus.Reopened;
            case SeasonalCostModelAllocationStatus.InReview:
                switch (next)
                {
                    case SeasonalCostModelAllocationStatus.Reopened:
                        return true;
                }
                return false;
            case SeasonalCostModelAllocationStatus.Resolved:
                switch (next)
                {
                    case SeasonalCostModelAllocationStatus.Reopened:
                        return true;
                }
                return false;
            case SeasonalCostModelAllocationStatus.Reopened:
                switch (next)
                {
                    case SeasonalCostModelAllocationStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalCostModelAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal cost model allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)BatchSize * (double)LeaveDays, 2);
    }

    public SeasonalCostModelAllocation Copy()
    {
        return new SeasonalCostModelAllocation(
            Code,
            Label,
            WindowDays,
            Checksum,
            Source,
            Timezone,
            ManagerId,
            LeaveDays,
            BatchSize,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
