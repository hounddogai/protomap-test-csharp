namespace Acme.Workforce.Compensation.MeritCycle.CycleOverride;

using Acme.Workforce.Kit.Text;

/// <summary>Where a merit cycle baseline is in its life cycle.</summary>
public enum MeritCycleBaselineStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a merit cycle baseline and its current status.</summary>
public sealed class MeritCycleBaseline(string code, string name, int updatedAt, double maximum, int attempts, MeritCycleBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Maximum { get; set; } = maximum;
    public int Attempts { get; set; } = attempts;
    public MeritCycleBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a merit cycle baseline in status proposed.</summary>
    public static MeritCycleBaseline NewMeritCycleBaseline(string code, string name)
    {
        return new MeritCycleBaseline(code, name, 0, 0.0, 0, MeritCycleBaselineStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MeritCycleBaselineStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MeritCycleBaselineStatus next)
    {
        switch (Status)
        {
            case MeritCycleBaselineStatus.Proposed:
                switch (next)
                {
                    case MeritCycleBaselineStatus.Rejected:
                        return true;
                }
                return false;
            case MeritCycleBaselineStatus.Approved:
                return next == MeritCycleBaselineStatus.Rejected || next == MeritCycleBaselineStatus.Withdrawn;
            case MeritCycleBaselineStatus.Rejected:
                switch (next)
                {
                    case MeritCycleBaselineStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(MeritCycleBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"merit cycle baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)Attempts - (double)UpdatedAt, 2);
    }

    public MeritCycleBaseline Copy()
    {
        return new MeritCycleBaseline(Code, Name, UpdatedAt, Maximum, Attempts, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
