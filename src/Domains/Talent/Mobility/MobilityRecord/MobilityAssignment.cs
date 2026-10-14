namespace Acme.Workforce.Talent.Mobility.MobilityRecord;

using Acme.Workforce.Kit.Text;

/// <summary>Where a mobility assignment is in its life cycle.</summary>
public enum MobilityAssignmentStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One mobility assignment, with what the planners need to act on it.</summary>
public sealed class MobilityAssignment(string code, string label, double discount, int expiresAt, int retries, double rate, double cost, MobilityAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Discount { get; set; } = discount;
    public int ExpiresAt { get; set; } = expiresAt;
    public int Retries { get; set; } = retries;
    public double Rate { get; set; } = rate;
    public double Cost { get; set; } = cost;
    public MobilityAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a mobility assignment in status created.</summary>
    public static MobilityAssignment NewMobilityAssignment(string code, string label)
    {
        return new MobilityAssignment(code, label, 0.0, 0, 0, 0.0, 0.0, MobilityAssignmentStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MobilityAssignmentStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MobilityAssignmentStatus next)
    {
        switch (Status)
        {
            case MobilityAssignmentStatus.Created:
                switch (next)
                {
                    case MobilityAssignmentStatus.Published:
                        return true;
                }
                return false;
            case MobilityAssignmentStatus.Validated:
                switch (next)
                {
                    case MobilityAssignmentStatus.Published:
                    case MobilityAssignmentStatus.Retired:
                        return true;
                }
                return false;
            case MobilityAssignmentStatus.Published:
                return next == MobilityAssignmentStatus.Retired;
        }
        return false;
    }

    public void MoveTo(MobilityAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"mobility assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double WeightedVolume()
    {
        return Math.Round(Cost * (double)ExpiresAt, 2);
    }

    public MobilityAssignment Copy()
    {
        return new MobilityAssignment(Code, Label, Discount, ExpiresAt, Retries, Rate, Cost, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
