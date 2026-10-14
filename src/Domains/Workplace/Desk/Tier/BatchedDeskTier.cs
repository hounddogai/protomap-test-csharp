namespace Acme.Workforce.Workplace.Desk.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched desk tier is in its life cycle.</summary>
public enum BatchedDeskTierStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A batched desk tier. Its code is unique within a tenant.</summary>
public sealed class BatchedDeskTier(string code, string label, string country, string region, string name, string grade, double amount, BatchedDeskTierStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Country { get; set; } = country;
    public string Region { get; set; } = region;
    public string Name { get; set; } = name;
    public string Grade { get; set; } = grade;
    public double Amount { get; set; } = amount;
    public BatchedDeskTierStatus Status { get; set; } = status;

    /// <summary>Starts a batched desk tier in status new.</summary>
    public static BatchedDeskTier NewBatchedDeskTier(string code, string label)
    {
        return new BatchedDeskTier(code, label, "", "", "", "", 0.0, BatchedDeskTierStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedDeskTierStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedDeskTierStatus next)
    {
        switch (Status)
        {
            case BatchedDeskTierStatus.New:
                return next == BatchedDeskTierStatus.Done;
            case BatchedDeskTierStatus.Queued:
                return next == BatchedDeskTierStatus.Done || next == BatchedDeskTierStatus.Failed;
            case BatchedDeskTierStatus.Running:
                switch (next)
                {
                    case BatchedDeskTierStatus.Done:
                    case BatchedDeskTierStatus.Failed:
                        return true;
                }
                return false;
            case BatchedDeskTierStatus.Done:
                switch (next)
                {
                    case BatchedDeskTierStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedDeskTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched desk tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public BatchedDeskTier Copy()
    {
        return new BatchedDeskTier(Code, Label, Country, Region, Name, Grade, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
