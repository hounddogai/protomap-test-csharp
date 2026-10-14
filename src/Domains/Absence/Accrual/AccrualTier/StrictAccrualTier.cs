namespace Acme.Workforce.Absence.Accrual.AccrualTier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict accrual tier is in its life cycle.</summary>
public enum StrictAccrualTierStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A strict accrual tier. Its code is unique within a tenant.</summary>
public sealed class StrictAccrualTier(string code, string title, string region, bool archived, string description, int quantity, StrictAccrualTierStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Region { get; set; } = region;
    public bool Archived { get; set; } = archived;
    public string Description { get; set; } = description;
    public int Quantity { get; set; } = quantity;
    public StrictAccrualTierStatus Status { get; set; } = status;

    /// <summary>Starts a strict accrual tier in status idle.</summary>
    public static StrictAccrualTier NewStrictAccrualTier(string code, string title)
    {
        return new StrictAccrualTier(code, title, "", false, "", 0, StrictAccrualTierStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictAccrualTierStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictAccrualTierStatus next)
    {
        switch (Status)
        {
            case StrictAccrualTierStatus.Idle:
                switch (next)
                {
                    case StrictAccrualTierStatus.Draining:
                        return true;
                }
                return false;
            case StrictAccrualTierStatus.Warming:
                switch (next)
                {
                    case StrictAccrualTierStatus.Stopped:
                        return true;
                }
                return false;
            case StrictAccrualTierStatus.Ready:
                switch (next)
                {
                    case StrictAccrualTierStatus.Draining:
                    case StrictAccrualTierStatus.Stopped:
                        return true;
                }
                return false;
            case StrictAccrualTierStatus.Draining:
                return next == StrictAccrualTierStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(StrictAccrualTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict accrual tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"strict accrual tier"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public StrictAccrualTier Copy()
    {
        return new StrictAccrualTier(Code, Title, Region, Archived, Description, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
