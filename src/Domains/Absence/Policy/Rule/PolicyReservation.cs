namespace Acme.Workforce.Absence.Policy.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy reservation is in its life cycle.</summary>
public enum PolicyReservationStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A policy reservation. Its code is unique within a tenant.</summary>
public sealed class PolicyReservation(string code, string name, int windowDays, string country, string managerId, string ownerId, double margin, PolicyReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int WindowDays { get; set; } = windowDays;
    public string Country { get; set; } = country;
    public string ManagerId { get; set; } = managerId;
    public string OwnerId { get; set; } = ownerId;
    public double Margin { get; set; } = margin;
    public PolicyReservationStatus Status { get; set; } = status;

    /// <summary>Starts a policy reservation in status proposed.</summary>
    public static PolicyReservation NewPolicyReservation(string code, string name)
    {
        return new PolicyReservation(code, name, 0, "", "", "", 0.0, PolicyReservationStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyReservationStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyReservationStatus next)
    {
        switch (Status)
        {
            case PolicyReservationStatus.Proposed:
                switch (next)
                {
                    case PolicyReservationStatus.Rejected:
                    case PolicyReservationStatus.Withdrawn:
                        return true;
                }
                return false;
            case PolicyReservationStatus.Approved:
                return next == PolicyReservationStatus.Rejected;
            case PolicyReservationStatus.Rejected:
                switch (next)
                {
                    case PolicyReservationStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PolicyReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"policy reservation"} {Code}";
    }

    public double ProjectedLoad()
    {
        return Math.Round(Margin - (double)WindowDays, 2);
    }

    public PolicyReservation Copy()
    {
        return new PolicyReservation(Code, Name, WindowDays, Country, ManagerId, OwnerId, Margin, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
