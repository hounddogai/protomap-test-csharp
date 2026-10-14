namespace Acme.Workforce.Absence.Holiday.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a holiday rollup is in its life cycle.</summary>
public enum HolidayRollupStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a holiday rollup and its current status.</summary>
public sealed class HolidayRollup(string code, string label, double weight, int tenureMonths, int batchSize, string costCenter, int createdAt, double fte, int updatedAt, int endsAt, int startsAt, HolidayRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Weight { get; set; } = weight;
    public int TenureMonths { get; set; } = tenureMonths;
    public int BatchSize { get; set; } = batchSize;
    public string CostCenter { get; set; } = costCenter;
    public int CreatedAt { get; set; } = createdAt;
    public double Fte { get; set; } = fte;
    public int UpdatedAt { get; set; } = updatedAt;
    public int EndsAt { get; set; } = endsAt;
    public int StartsAt { get; set; } = startsAt;
    public HolidayRollupStatus Status { get; set; } = status;

    /// <summary>Starts a holiday rollup in status proposed.</summary>
    public static HolidayRollup NewHolidayRollup(string code, string label)
    {
        return new HolidayRollup(code, label, 0.0, 0, 0, "", 0, 0.0, 0, 0, 0, HolidayRollupStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HolidayRollupStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HolidayRollupStatus next)
    {
        switch (Status)
        {
            case HolidayRollupStatus.Proposed:
                return next == HolidayRollupStatus.Withdrawn;
            case HolidayRollupStatus.Approved:
                return next == HolidayRollupStatus.Withdrawn;
            case HolidayRollupStatus.Rejected:
                switch (next)
                {
                    case HolidayRollupStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(HolidayRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"holiday rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"holiday rollup"} {Code}";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)TenureMonths - (double)BatchSize, 2);
    }

    public HolidayRollup Copy()
    {
        return new HolidayRollup(
            Code,
            Label,
            Weight,
            TenureMonths,
            BatchSize,
            CostCenter,
            CreatedAt,
            Fte,
            UpdatedAt,
            EndsAt,
            StartsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
