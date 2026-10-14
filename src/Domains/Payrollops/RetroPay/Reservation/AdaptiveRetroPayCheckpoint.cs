namespace Acme.Workforce.Payrollops.RetroPay.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive retro pay checkpoint is in its life cycle.</summary>
public enum AdaptiveRetroPayCheckpointStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a adaptive retro pay checkpoint and its current status.</summary>
public sealed class AdaptiveRetroPayCheckpoint(string code, string name, double cost, string managerId, int limit, double threshold, AdaptiveRetroPayCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Cost { get; set; } = cost;
    public string ManagerId { get; set; } = managerId;
    public int Limit { get; set; } = limit;
    public double Threshold { get; set; } = threshold;
    public AdaptiveRetroPayCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive retro pay checkpoint in status created.</summary>
    public static AdaptiveRetroPayCheckpoint NewAdaptiveRetroPayCheckpoint(string code, string name)
    {
        return new AdaptiveRetroPayCheckpoint(code, name, 0.0, "", 0, 0.0, AdaptiveRetroPayCheckpointStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveRetroPayCheckpointStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveRetroPayCheckpointStatus next)
    {
        switch (Status)
        {
            case AdaptiveRetroPayCheckpointStatus.Created:
                switch (next)
                {
                    case AdaptiveRetroPayCheckpointStatus.Published:
                    case AdaptiveRetroPayCheckpointStatus.Retired:
                        return true;
                }
                return false;
            case AdaptiveRetroPayCheckpointStatus.Validated:
                switch (next)
                {
                    case AdaptiveRetroPayCheckpointStatus.Published:
                    case AdaptiveRetroPayCheckpointStatus.Retired:
                        return true;
                }
                return false;
            case AdaptiveRetroPayCheckpointStatus.Published:
                return next == AdaptiveRetroPayCheckpointStatus.Retired;
        }
        return false;
    }

    public void MoveTo(AdaptiveRetroPayCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive retro pay checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double NetValue()
    {
        return Math.Round((double)Limit + Cost, 2);
    }

    public AdaptiveRetroPayCheckpoint Copy()
    {
        return new AdaptiveRetroPayCheckpoint(Code, Name, Cost, ManagerId, Limit, Threshold, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
