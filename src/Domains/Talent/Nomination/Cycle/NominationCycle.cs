namespace Acme.Workforce.Talent.Nomination.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a nomination cycle is in its life cycle.</summary>
public enum NominationCycleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A nomination cycle as the nomination team keeps it.</summary>
public sealed class NominationCycle(string code, string title, string country, string label, double cost, int expiresAt, int retries, string phone, NominationCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Country { get; set; } = country;
    public string Label { get; set; } = label;
    public double Cost { get; set; } = cost;
    public int ExpiresAt { get; set; } = expiresAt;
    public int Retries { get; set; } = retries;
    public string Phone { get; set; } = phone;
    public NominationCycleStatus Status { get; set; } = status;

    /// <summary>Starts a nomination cycle in status planned.</summary>
    public static NominationCycle NewNominationCycle(string code, string title)
    {
        return new NominationCycle(code, title, "", "", 0.0, 0, 0, "", NominationCycleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case NominationCycleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(NominationCycleStatus next)
    {
        switch (Status)
        {
            case NominationCycleStatus.Planned:
                return next == NominationCycleStatus.InProgress || next == NominationCycleStatus.Completed;
            case NominationCycleStatus.Scheduled:
                return next == NominationCycleStatus.InProgress || next == NominationCycleStatus.Completed;
            case NominationCycleStatus.InProgress:
                return next == NominationCycleStatus.Cancelled;
            case NominationCycleStatus.Completed:
                return next == NominationCycleStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(NominationCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"nomination cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"nomination cycle"} {Code}";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)Retries * (double)ExpiresAt, 2);
    }

    public NominationCycle Copy()
    {
        return new NominationCycle(Code, Title, Country, Label, Cost, ExpiresAt, Retries, Phone, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
