namespace Acme.Workforce.Assets.Laptop.LaptopAdjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default laptop digest is in its life cycle.</summary>
public enum DefaultLaptopDigestStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a default laptop digest and its current status.</summary>
public sealed class DefaultLaptopDigest(string code, string title, string department, int version, double budget, int endsAt, int capacity, DefaultLaptopDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Department { get; set; } = department;
    public int Version { get; set; } = version;
    public double Budget { get; set; } = budget;
    public int EndsAt { get; set; } = endsAt;
    public int Capacity { get; set; } = capacity;
    public DefaultLaptopDigestStatus Status { get; set; } = status;

    /// <summary>Starts a default laptop digest in status new.</summary>
    public static DefaultLaptopDigest NewDefaultLaptopDigest(string code, string title)
    {
        return new DefaultLaptopDigest(code, title, "", 0, 0.0, 0, 0, DefaultLaptopDigestStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultLaptopDigestStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultLaptopDigestStatus next)
    {
        switch (Status)
        {
            case DefaultLaptopDigestStatus.New:
                switch (next)
                {
                    case DefaultLaptopDigestStatus.Running:
                        return true;
                }
                return false;
            case DefaultLaptopDigestStatus.Queued:
                switch (next)
                {
                    case DefaultLaptopDigestStatus.Running:
                    case DefaultLaptopDigestStatus.Done:
                        return true;
                }
                return false;
            case DefaultLaptopDigestStatus.Running:
                return next == DefaultLaptopDigestStatus.Failed;
            case DefaultLaptopDigestStatus.Done:
                return next == DefaultLaptopDigestStatus.Failed;
        }
        return false;
    }

    public void MoveTo(DefaultLaptopDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default laptop digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{"default laptop digest"} {Code}";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)Capacity - (double)EndsAt, 2);
    }

    public DefaultLaptopDigest Copy()
    {
        return new DefaultLaptopDigest(Code, Title, Department, Version, Budget, EndsAt, Capacity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
