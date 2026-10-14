namespace Acme.Workforce.Compliance.PolicyAck.AckProfile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted policy ack profile is in its life cycle.</summary>
public enum WeightedPolicyAckProfileStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a weighted policy ack profile and its current status.</summary>
public sealed class WeightedPolicyAckProfile(string code, string title, double weight, string country, string notes, double fte, int startsAt, WeightedPolicyAckProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Weight { get; set; } = weight;
    public string Country { get; set; } = country;
    public string Notes { get; set; } = notes;
    public double Fte { get; set; } = fte;
    public int StartsAt { get; set; } = startsAt;
    public WeightedPolicyAckProfileStatus Status { get; set; } = status;

    /// <summary>Starts a weighted policy ack profile in status created.</summary>
    public static WeightedPolicyAckProfile NewWeightedPolicyAckProfile(string code, string title)
    {
        return new WeightedPolicyAckProfile(
            code,
            title,
            0.0,
            "",
            "",
            0.0,
            0,
            WeightedPolicyAckProfileStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedPolicyAckProfileStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedPolicyAckProfileStatus next)
    {
        switch (Status)
        {
            case WeightedPolicyAckProfileStatus.Created:
                switch (next)
                {
                    case WeightedPolicyAckProfileStatus.Validated:
                    case WeightedPolicyAckProfileStatus.Published:
                        return true;
                }
                return false;
            case WeightedPolicyAckProfileStatus.Validated:
                return next == WeightedPolicyAckProfileStatus.Retired;
            case WeightedPolicyAckProfileStatus.Published:
                return next == WeightedPolicyAckProfileStatus.Retired;
        }
        return false;
    }

    public void MoveTo(WeightedPolicyAckProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted policy ack profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Fte + (double)StartsAt, 2);
    }

    public WeightedPolicyAckProfile Copy()
    {
        return new WeightedPolicyAckProfile(Code, Title, Weight, Country, Notes, Fte, StartsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
