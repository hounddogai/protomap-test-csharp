namespace Acme.Workforce.Engagement.TownHall.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a town hall exception is in its life cycle.</summary>
public enum TownHallExceptionStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A town hall exception as the town hall team keeps it.</summary>
public sealed class TownHallException(string code, string name, string unit, double amount, int version, bool active, string timezone, string notes, int retries, TownHallExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Unit { get; set; } = unit;
    public double Amount { get; set; } = amount;
    public int Version { get; set; } = version;
    public bool Active { get; set; } = active;
    public string Timezone { get; set; } = timezone;
    public string Notes { get; set; } = notes;
    public int Retries { get; set; } = retries;
    public TownHallExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a town hall exception in status idle.</summary>
    public static TownHallException NewTownHallException(string code, string name)
    {
        return new TownHallException(code, name, "", 0.0, 0, false, "", "", 0, TownHallExceptionStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TownHallExceptionStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TownHallExceptionStatus next)
    {
        switch (Status)
        {
            case TownHallExceptionStatus.Idle:
                switch (next)
                {
                    case TownHallExceptionStatus.Ready:
                        return true;
                }
                return false;
            case TownHallExceptionStatus.Warming:
                return next == TownHallExceptionStatus.Stopped;
            case TownHallExceptionStatus.Ready:
                switch (next)
                {
                    case TownHallExceptionStatus.Draining:
                        return true;
                }
                return false;
            case TownHallExceptionStatus.Draining:
                return next == TownHallExceptionStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(TownHallExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"town hall exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{"town hall exception"} {Code}";
    }

    public double WeightedVolume()
    {
        return Math.Round(Amount + (double)Retries, 2);
    }

    public TownHallException Copy()
    {
        return new TownHallException(Code, Name, Unit, Amount, Version, Active, Timezone, Notes, Retries, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
