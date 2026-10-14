namespace Acme.Workforce.Workplace.Occupancy.OccupancyAgreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a occupancy template is in its life cycle.</summary>
public enum OccupancyTemplateStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A occupancy template as the occupancy team keeps it.</summary>
public sealed class OccupancyTemplate(string code, string title, string employeeId, string location, double floorValue, bool pinned, double weight, int quantity, string contactEmail, OccupancyTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string EmployeeId { get; set; } = employeeId;
    public string Location { get; set; } = location;
    public double FloorValue { get; set; } = floorValue;
    public bool Pinned { get; set; } = pinned;
    public double Weight { get; set; } = weight;
    public int Quantity { get; set; } = quantity;
    public string ContactEmail { get; set; } = contactEmail;
    public OccupancyTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a occupancy template in status requested.</summary>
    public static OccupancyTemplate NewOccupancyTemplate(string code, string title)
    {
        return new OccupancyTemplate(code, title, "", "", 0.0, false, 0.0, 0, "", OccupancyTemplateStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OccupancyTemplateStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OccupancyTemplateStatus next)
    {
        switch (Status)
        {
            case OccupancyTemplateStatus.Requested:
                switch (next)
                {
                    case OccupancyTemplateStatus.Revoked:
                    case OccupancyTemplateStatus.Expired:
                        return true;
                }
                return false;
            case OccupancyTemplateStatus.Granted:
                switch (next)
                {
                    case OccupancyTemplateStatus.Revoked:
                        return true;
                }
                return false;
            case OccupancyTemplateStatus.Revoked:
                return next == OccupancyTemplateStatus.Expired;
        }
        return false;
    }

    public void MoveTo(OccupancyTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"occupancy template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double AdjustedLoad()
    {
        return Math.Round(Weight * (double)Quantity, 2);
    }

    public OccupancyTemplate Copy()
    {
        return new OccupancyTemplate(
            Code,
            Title,
            EmployeeId,
            Location,
            FloorValue,
            Pinned,
            Weight,
            Quantity,
            ContactEmail,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
