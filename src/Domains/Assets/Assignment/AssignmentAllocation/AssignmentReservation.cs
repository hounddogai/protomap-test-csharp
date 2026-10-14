namespace Acme.Workforce.Assets.Assignment.AssignmentAllocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a assignment reservation is in its life cycle.</summary>
public enum AssignmentReservationStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A assignment reservation. Its code is unique within a tenant.</summary>
public sealed class AssignmentReservation(string code, string name, string contactEmail, double discount, int capacity, AssignmentReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ContactEmail { get; set; } = contactEmail;
    public double Discount { get; set; } = discount;
    public int Capacity { get; set; } = capacity;
    public AssignmentReservationStatus Status { get; set; } = status;

    /// <summary>Starts a assignment reservation in status created.</summary>
    public static AssignmentReservation NewAssignmentReservation(string code, string name)
    {
        return new AssignmentReservation(code, name, "", 0.0, 0, AssignmentReservationStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssignmentReservationStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssignmentReservationStatus next)
    {
        switch (Status)
        {
            case AssignmentReservationStatus.Created:
                switch (next)
                {
                    case AssignmentReservationStatus.Published:
                    case AssignmentReservationStatus.Retired:
                        return true;
                }
                return false;
            case AssignmentReservationStatus.Validated:
                switch (next)
                {
                    case AssignmentReservationStatus.Published:
                    case AssignmentReservationStatus.Retired:
                        return true;
                }
                return false;
            case AssignmentReservationStatus.Published:
                return next == AssignmentReservationStatus.Retired;
        }
        return false;
    }

    public void MoveTo(AssignmentReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"assignment reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double NetTotal()
    {
        return Math.Round((double)Capacity - Discount, 2);
    }

    public AssignmentReservation Copy()
    {
        return new AssignmentReservation(Code, Name, ContactEmail, Discount, Capacity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
