namespace Acme.Workforce.Assets.Warranty.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a warranty request is in its life cycle.</summary>
public enum WarrantyRequestStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a warranty request and its current status.</summary>
public sealed class WarrantyRequest(string code, string title, string checksum, string contactEmail, int capacity, int startsAt, string label, double floorValue, WarrantyRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Checksum { get; set; } = checksum;
    public string ContactEmail { get; set; } = contactEmail;
    public int Capacity { get; set; } = capacity;
    public int StartsAt { get; set; } = startsAt;
    public string Label { get; set; } = label;
    public double FloorValue { get; set; } = floorValue;
    public WarrantyRequestStatus Status { get; set; } = status;

    /// <summary>Starts a warranty request in status proposed.</summary>
    public static WarrantyRequest NewWarrantyRequest(string code, string title)
    {
        return new WarrantyRequest(code, title, "", "", 0, 0, "", 0.0, WarrantyRequestStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WarrantyRequestStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WarrantyRequestStatus next)
    {
        switch (Status)
        {
            case WarrantyRequestStatus.Proposed:
                return next == WarrantyRequestStatus.Withdrawn;
            case WarrantyRequestStatus.Approved:
                return next == WarrantyRequestStatus.Rejected || next == WarrantyRequestStatus.Withdrawn;
            case WarrantyRequestStatus.Rejected:
                return next == WarrantyRequestStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(WarrantyRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"warranty request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Title}";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)StartsAt + FloorValue, 2);
    }

    public WarrantyRequest Copy()
    {
        return new WarrantyRequest(
            Code,
            Title,
            Checksum,
            ContactEmail,
            Capacity,
            StartsAt,
            Label,
            FloorValue,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
