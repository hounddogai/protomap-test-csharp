namespace Acme.Workforce.Talent.Mobility.MobilityReservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a mobility policy is in its life cycle.</summary>
public enum MobilityPolicyStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A mobility policy. Its code is unique within a tenant.</summary>
public sealed class MobilityPolicy(string code, string name, int createdAt, bool active, double budget, double weight, MobilityPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int CreatedAt { get; set; } = createdAt;
    public bool Active { get; set; } = active;
    public double Budget { get; set; } = budget;
    public double Weight { get; set; } = weight;
    public MobilityPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a mobility policy in status created.</summary>
    public static MobilityPolicy NewMobilityPolicy(string code, string name)
    {
        return new MobilityPolicy(code, name, 0, false, 0.0, 0.0, MobilityPolicyStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MobilityPolicyStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MobilityPolicyStatus next)
    {
        switch (Status)
        {
            case MobilityPolicyStatus.Created:
                switch (next)
                {
                    case MobilityPolicyStatus.Published:
                    case MobilityPolicyStatus.Retired:
                        return true;
                }
                return false;
            case MobilityPolicyStatus.Validated:
                switch (next)
                {
                    case MobilityPolicyStatus.Published:
                    case MobilityPolicyStatus.Retired:
                        return true;
                }
                return false;
            case MobilityPolicyStatus.Published:
                switch (next)
                {
                    case MobilityPolicyStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(MobilityPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"mobility policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double WeightedValue()
    {
        return Math.Round(Budget - Weight, 2);
    }

    public MobilityPolicy Copy()
    {
        return new MobilityPolicy(Code, Name, CreatedAt, Active, Budget, Weight, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
