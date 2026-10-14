namespace Acme.Workforce.Orgdesign.CostModel.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted cost model slot is in its life cycle.</summary>
public enum WeightedCostModelSlotStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A weighted cost model slot as the cost model team keeps it.</summary>
public sealed class WeightedCostModelSlot(string code, string name, string country, int batchSize, int retries, string timezone, WeightedCostModelSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Country { get; set; } = country;
    public int BatchSize { get; set; } = batchSize;
    public int Retries { get; set; } = retries;
    public string Timezone { get; set; } = timezone;
    public WeightedCostModelSlotStatus Status { get; set; } = status;

    /// <summary>Starts a weighted cost model slot in status planned.</summary>
    public static WeightedCostModelSlot NewWeightedCostModelSlot(string code, string name)
    {
        return new WeightedCostModelSlot(code, name, "", 0, 0, "", WeightedCostModelSlotStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedCostModelSlotStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedCostModelSlotStatus next)
    {
        switch (Status)
        {
            case WeightedCostModelSlotStatus.Planned:
                return next == WeightedCostModelSlotStatus.Completed;
            case WeightedCostModelSlotStatus.Scheduled:
                switch (next)
                {
                    case WeightedCostModelSlotStatus.Completed:
                        return true;
                }
                return false;
            case WeightedCostModelSlotStatus.InProgress:
                return next == WeightedCostModelSlotStatus.Completed || next == WeightedCostModelSlotStatus.Cancelled;
            case WeightedCostModelSlotStatus.Completed:
                switch (next)
                {
                    case WeightedCostModelSlotStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedCostModelSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted cost model slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"weighted cost model slot"} {Code}";
    }

    public double NetValue()
    {
        return Math.Round((double)BatchSize - (double)Retries, 2);
    }

    public WeightedCostModelSlot Copy()
    {
        return new WeightedCostModelSlot(Code, Name, Country, BatchSize, Retries, Timezone, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
