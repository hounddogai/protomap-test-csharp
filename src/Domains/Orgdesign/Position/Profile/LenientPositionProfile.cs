namespace Acme.Workforce.Orgdesign.Position.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient position profile is in its life cycle.</summary>
public enum LenientPositionProfileStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a lenient position profile and its current status.</summary>
public sealed class LenientPositionProfile(string code, string title, List<string> tags, double threshold, string badge, string unit, double cost, string name, string jobCode, LenientPositionProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public List<string> Tags { get; set; } = tags;
    public double Threshold { get; set; } = threshold;
    public string Badge { get; set; } = badge;
    public string Unit { get; set; } = unit;
    public double Cost { get; set; } = cost;
    public string Name { get; set; } = name;
    public string JobCode { get; set; } = jobCode;
    public LenientPositionProfileStatus Status { get; set; } = status;

    /// <summary>Starts a lenient position profile in status proposed.</summary>
    public static LenientPositionProfile NewLenientPositionProfile(string code, string title)
    {
        return new LenientPositionProfile(
            code,
            title,
            new List<string>(),
            0.0,
            "",
            "",
            0.0,
            "",
            "",
            LenientPositionProfileStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientPositionProfileStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientPositionProfileStatus next)
    {
        switch (Status)
        {
            case LenientPositionProfileStatus.Proposed:
                switch (next)
                {
                    case LenientPositionProfileStatus.Approved:
                    case LenientPositionProfileStatus.Withdrawn:
                        return true;
                }
                return false;
            case LenientPositionProfileStatus.Approved:
                switch (next)
                {
                    case LenientPositionProfileStatus.Rejected:
                    case LenientPositionProfileStatus.Withdrawn:
                        return true;
                }
                return false;
            case LenientPositionProfileStatus.Rejected:
                return next == LenientPositionProfileStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(LenientPositionProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient position profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"lenient position profile"} {Code}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double NetValue()
    {
        return Math.Round(Threshold - Cost, 2);
    }

    public LenientPositionProfile Copy()
    {
        return new LenientPositionProfile(Code, Title, Tags, Threshold, Badge, Unit, Cost, Name, JobCode, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
