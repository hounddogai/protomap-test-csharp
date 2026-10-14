namespace Acme.Workforce.Compensation.MeritCycle.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a merit cycle assignment is in its life cycle.</summary>
public enum MeritCycleAssignmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A merit cycle assignment as the merit cycle team keeps it.</summary>
public sealed class MeritCycleAssignment(string code, string name, string source, string location, int expiresAt, int quantity, string phone, bool visible, MeritCycleAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Source { get; set; } = source;
    public string Location { get; set; } = location;
    public int ExpiresAt { get; set; } = expiresAt;
    public int Quantity { get; set; } = quantity;
    public string Phone { get; set; } = phone;
    public bool Visible { get; set; } = visible;
    public MeritCycleAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a merit cycle assignment in status idle.</summary>
    public static MeritCycleAssignment NewMeritCycleAssignment(string code, string name)
    {
        return new MeritCycleAssignment(code, name, "", "", 0, 0, "", false, MeritCycleAssignmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MeritCycleAssignmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MeritCycleAssignmentStatus next)
    {
        switch (Status)
        {
            case MeritCycleAssignmentStatus.Idle:
                return next == MeritCycleAssignmentStatus.Draining;
            case MeritCycleAssignmentStatus.Warming:
                switch (next)
                {
                    case MeritCycleAssignmentStatus.Ready:
                    case MeritCycleAssignmentStatus.Draining:
                        return true;
                }
                return false;
            case MeritCycleAssignmentStatus.Ready:
                switch (next)
                {
                    case MeritCycleAssignmentStatus.Stopped:
                        return true;
                }
                return false;
            case MeritCycleAssignmentStatus.Draining:
                switch (next)
                {
                    case MeritCycleAssignmentStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(MeritCycleAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"merit cycle assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)Quantity - (double)ExpiresAt, 2);
    }

    public MeritCycleAssignment Copy()
    {
        return new MeritCycleAssignment(Code, Name, Source, Location, ExpiresAt, Quantity, Phone, Visible, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
