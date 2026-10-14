namespace Acme.Workforce.Orgdesign.CostModel.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a cost model entry is in its life cycle.</summary>
public enum CostModelEntryStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a cost model entry and its current status.</summary>
public sealed class CostModelEntry(string code, string name, string checksum, string region, double ratio, string department, string ownerId, bool approved, int quantity, string channel, CostModelEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Checksum { get; set; } = checksum;
    public string Region { get; set; } = region;
    public double Ratio { get; set; } = ratio;
    public string Department { get; set; } = department;
    public string OwnerId { get; set; } = ownerId;
    public bool Approved { get; set; } = approved;
    public int Quantity { get; set; } = quantity;
    public string Channel { get; set; } = channel;
    public CostModelEntryStatus Status { get; set; } = status;

    /// <summary>Starts a cost model entry in status requested.</summary>
    public static CostModelEntry NewCostModelEntry(string code, string name)
    {
        return new CostModelEntry(code, name, "", "", 0.0, "", "", false, 0, "", CostModelEntryStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CostModelEntryStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CostModelEntryStatus next)
    {
        switch (Status)
        {
            case CostModelEntryStatus.Requested:
                switch (next)
                {
                    case CostModelEntryStatus.Expired:
                        return true;
                }
                return false;
            case CostModelEntryStatus.Granted:
                switch (next)
                {
                    case CostModelEntryStatus.Revoked:
                    case CostModelEntryStatus.Expired:
                        return true;
                }
                return false;
            case CostModelEntryStatus.Revoked:
                return next == CostModelEntryStatus.Expired;
        }
        return false;
    }

    public void MoveTo(CostModelEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"cost model entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Ratio * (double)Quantity, 2);
    }

    public CostModelEntry Copy()
    {
        return new CostModelEntry(
            Code,
            Name,
            Checksum,
            Region,
            Ratio,
            Department,
            OwnerId,
            Approved,
            Quantity,
            Channel,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
