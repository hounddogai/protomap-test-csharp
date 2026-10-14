namespace Acme.Workforce.Assets.Laptop.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a laptop digest is in its life cycle.</summary>
public enum LaptopDigestStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One laptop digest, with what the planners need to act on it.</summary>
public sealed class LaptopDigest(string code, string name, string title, string timezone, double minimum, bool approved, double weight, int updatedAt, double rate, LaptopDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Title { get; set; } = title;
    public string Timezone { get; set; } = timezone;
    public double Minimum { get; set; } = minimum;
    public bool Approved { get; set; } = approved;
    public double Weight { get; set; } = weight;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Rate { get; set; } = rate;
    public LaptopDigestStatus Status { get; set; } = status;

    /// <summary>Starts a laptop digest in status idle.</summary>
    public static LaptopDigest NewLaptopDigest(string code, string name)
    {
        return new LaptopDigest(code, name, "", "", 0.0, false, 0.0, 0, 0.0, LaptopDigestStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LaptopDigestStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LaptopDigestStatus next)
    {
        switch (Status)
        {
            case LaptopDigestStatus.Idle:
                return next == LaptopDigestStatus.Stopped;
            case LaptopDigestStatus.Warming:
                switch (next)
                {
                    case LaptopDigestStatus.Stopped:
                        return true;
                }
                return false;
            case LaptopDigestStatus.Ready:
                switch (next)
                {
                    case LaptopDigestStatus.Draining:
                    case LaptopDigestStatus.Stopped:
                        return true;
                }
                return false;
            case LaptopDigestStatus.Draining:
                return next == LaptopDigestStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(LaptopDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"laptop digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)UpdatedAt - Weight, 2);
    }

    public LaptopDigest Copy()
    {
        return new LaptopDigest(Code, Name, Title, Timezone, Minimum, Approved, Weight, UpdatedAt, Rate, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
