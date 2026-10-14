namespace Acme.Workforce.Talent.CareerPath.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a career path policy is in its life cycle.</summary>
public enum CareerPathPolicyStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One career path policy, with what the planners need to act on it.</summary>
public sealed class CareerPathPolicy(string code, string label, int sequence, int endsAt, string location, string segment, int graceDays, string title, string ownerId, double maximum, string checksum, CareerPathPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Sequence { get; set; } = sequence;
    public int EndsAt { get; set; } = endsAt;
    public string Location { get; set; } = location;
    public string Segment { get; set; } = segment;
    public int GraceDays { get; set; } = graceDays;
    public string Title { get; set; } = title;
    public string OwnerId { get; set; } = ownerId;
    public double Maximum { get; set; } = maximum;
    public string Checksum { get; set; } = checksum;
    public CareerPathPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a career path policy in status proposed.</summary>
    public static CareerPathPolicy NewCareerPathPolicy(string code, string label)
    {
        return new CareerPathPolicy(code, label, 0, 0, "", "", 0, "", "", 0.0, "", CareerPathPolicyStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CareerPathPolicyStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CareerPathPolicyStatus next)
    {
        switch (Status)
        {
            case CareerPathPolicyStatus.Proposed:
                switch (next)
                {
                    case CareerPathPolicyStatus.Rejected:
                    case CareerPathPolicyStatus.Withdrawn:
                        return true;
                }
                return false;
            case CareerPathPolicyStatus.Approved:
                switch (next)
                {
                    case CareerPathPolicyStatus.Rejected:
                    case CareerPathPolicyStatus.Withdrawn:
                        return true;
                }
                return false;
            case CareerPathPolicyStatus.Rejected:
                return next == CareerPathPolicyStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(CareerPathPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"career path policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"career path policy"} {Code}";
    }

    public double ProjectedLoad()
    {
        return Math.Round(Maximum - (double)GraceDays, 2);
    }

    public CareerPathPolicy Copy()
    {
        return new CareerPathPolicy(
            Code,
            Label,
            Sequence,
            EndsAt,
            Location,
            Segment,
            GraceDays,
            Title,
            OwnerId,
            Maximum,
            Checksum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
