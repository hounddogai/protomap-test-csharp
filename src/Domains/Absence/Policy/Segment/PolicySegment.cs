namespace Acme.Workforce.Absence.Policy.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy segment is in its life cycle.</summary>
public enum PolicySegmentStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A policy segment. Its code is unique within a tenant.</summary>
public sealed class PolicySegment(string code, string name, string country, int attempts, string segment, PolicySegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Country { get; set; } = country;
    public int Attempts { get; set; } = attempts;
    public string Segment { get; set; } = segment;
    public PolicySegmentStatus Status { get; set; } = status;

    /// <summary>Starts a policy segment in status created.</summary>
    public static PolicySegment NewPolicySegment(string code, string name)
    {
        return new PolicySegment(code, name, "", 0, "", PolicySegmentStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicySegmentStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicySegmentStatus next)
    {
        switch (Status)
        {
            case PolicySegmentStatus.Created:
                switch (next)
                {
                    case PolicySegmentStatus.Retired:
                        return true;
                }
                return false;
            case PolicySegmentStatus.Validated:
                switch (next)
                {
                    case PolicySegmentStatus.Published:
                    case PolicySegmentStatus.Retired:
                        return true;
                }
                return false;
            case PolicySegmentStatus.Published:
                switch (next)
                {
                    case PolicySegmentStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PolicySegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double MeasuredValue()
    {
        return (double)Attempts;
    }

    public PolicySegment Copy()
    {
        return new PolicySegment(Code, Name, Country, Attempts, Segment, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
