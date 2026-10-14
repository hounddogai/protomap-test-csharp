namespace Acme.Workforce.Assets.Laptop.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a laptop segment is in its life cycle.</summary>
public enum LaptopSegmentStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A laptop segment as the laptop team keeps it.</summary>
public sealed class LaptopSegment(string code, string name, string skill, double floorValue, double margin, string summary, LaptopSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Skill { get; set; } = skill;
    public double FloorValue { get; set; } = floorValue;
    public double Margin { get; set; } = margin;
    public string Summary { get; set; } = summary;
    public LaptopSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a laptop segment in status requested.</summary>
    public static LaptopSegment NewLaptopSegment(string code, string name)
    {
        return new LaptopSegment(code, name, "", 0.0, 0.0, "", LaptopSegmentStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LaptopSegmentStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LaptopSegmentStatus next)
    {
        switch (Status)
        {
            case LaptopSegmentStatus.Requested:
                switch (next)
                {
                    case LaptopSegmentStatus.Expired:
                        return true;
                }
                return false;
            case LaptopSegmentStatus.Granted:
                switch (next)
                {
                    case LaptopSegmentStatus.Revoked:
                        return true;
                }
                return false;
            case LaptopSegmentStatus.Revoked:
                return next == LaptopSegmentStatus.Expired;
        }
        return false;
    }

    public void MoveTo(LaptopSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"laptop segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"laptop segment"} {Code}";
    }

    public double EffectiveLoad()
    {
        return Math.Round(FloorValue * Margin, 2);
    }

    public LaptopSegment Copy()
    {
        return new LaptopSegment(Code, Name, Skill, FloorValue, Margin, Summary, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
