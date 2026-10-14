namespace Acme.Workforce.Compensation.PayBand.BandBaseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global pay band baseline is in its life cycle.</summary>
public enum GlobalPayBandBaselineStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A global pay band baseline. Its code is unique within a tenant.</summary>
public sealed class GlobalPayBandBaseline(string code, string name, string ownerId, int updatedAt, int quantity, GlobalPayBandBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string OwnerId { get; set; } = ownerId;
    public int UpdatedAt { get; set; } = updatedAt;
    public int Quantity { get; set; } = quantity;
    public GlobalPayBandBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a global pay band baseline in status planned.</summary>
    public static GlobalPayBandBaseline NewGlobalPayBandBaseline(string code, string name)
    {
        return new GlobalPayBandBaseline(code, name, "", 0, 0, GlobalPayBandBaselineStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalPayBandBaselineStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalPayBandBaselineStatus next)
    {
        switch (Status)
        {
            case GlobalPayBandBaselineStatus.Planned:
                return next == GlobalPayBandBaselineStatus.Completed;
            case GlobalPayBandBaselineStatus.Scheduled:
                switch (next)
                {
                    case GlobalPayBandBaselineStatus.Completed:
                    case GlobalPayBandBaselineStatus.Cancelled:
                        return true;
                }
                return false;
            case GlobalPayBandBaselineStatus.InProgress:
                switch (next)
                {
                    case GlobalPayBandBaselineStatus.Completed:
                        return true;
                }
                return false;
            case GlobalPayBandBaselineStatus.Completed:
                switch (next)
                {
                    case GlobalPayBandBaselineStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GlobalPayBandBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global pay band baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)Quantity * (double)UpdatedAt, 2);
    }

    public GlobalPayBandBaseline Copy()
    {
        return new GlobalPayBandBaseline(Code, Name, OwnerId, UpdatedAt, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
