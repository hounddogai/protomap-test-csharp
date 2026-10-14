namespace Acme.Workforce.Absence.Policy.WeightedPolicyPreference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict policy entry is in its life cycle.</summary>
public enum StrictPolicyEntryStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a strict policy entry and its current status.</summary>
public sealed class StrictPolicyEntry(string code, string name, string ownerId, double minimum, double discount, int retries, string location, int updatedAt, int quantity, StrictPolicyEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string OwnerId { get; set; } = ownerId;
    public double Minimum { get; set; } = minimum;
    public double Discount { get; set; } = discount;
    public int Retries { get; set; } = retries;
    public string Location { get; set; } = location;
    public int UpdatedAt { get; set; } = updatedAt;
    public int Quantity { get; set; } = quantity;
    public StrictPolicyEntryStatus Status { get; set; } = status;

    /// <summary>Starts a strict policy entry in status requested.</summary>
    public static StrictPolicyEntry NewStrictPolicyEntry(string code, string name)
    {
        return new StrictPolicyEntry(code, name, "", 0.0, 0.0, 0, "", 0, 0, StrictPolicyEntryStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictPolicyEntryStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictPolicyEntryStatus next)
    {
        switch (Status)
        {
            case StrictPolicyEntryStatus.Requested:
                switch (next)
                {
                    case StrictPolicyEntryStatus.Revoked:
                        return true;
                }
                return false;
            case StrictPolicyEntryStatus.Granted:
                return next == StrictPolicyEntryStatus.Revoked || next == StrictPolicyEntryStatus.Expired;
            case StrictPolicyEntryStatus.Revoked:
                switch (next)
                {
                    case StrictPolicyEntryStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictPolicyEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict policy entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)Quantity - (double)UpdatedAt, 2);
    }

    public StrictPolicyEntry Copy()
    {
        return new StrictPolicyEntry(
            Code,
            Name,
            OwnerId,
            Minimum,
            Discount,
            Retries,
            Location,
            UpdatedAt,
            Quantity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
