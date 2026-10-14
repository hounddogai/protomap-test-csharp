namespace Acme.Workforce.Orgdesign.Reorg.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a reorg mapping is in its life cycle.</summary>
public enum ReorgMappingStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A reorg mapping as the reorg team keeps it.</summary>
public sealed class ReorgMapping(string code, string name, string country, string title, double weight, int expiresAt, ReorgMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Country { get; set; } = country;
    public string Title { get; set; } = title;
    public double Weight { get; set; } = weight;
    public int ExpiresAt { get; set; } = expiresAt;
    public ReorgMappingStatus Status { get; set; } = status;

    /// <summary>Starts a reorg mapping in status idle.</summary>
    public static ReorgMapping NewReorgMapping(string code, string name)
    {
        return new ReorgMapping(code, name, "", "", 0.0, 0, ReorgMappingStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReorgMappingStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReorgMappingStatus next)
    {
        switch (Status)
        {
            case ReorgMappingStatus.Idle:
                switch (next)
                {
                    case ReorgMappingStatus.Draining:
                    case ReorgMappingStatus.Stopped:
                        return true;
                }
                return false;
            case ReorgMappingStatus.Warming:
                switch (next)
                {
                    case ReorgMappingStatus.Ready:
                    case ReorgMappingStatus.Stopped:
                        return true;
                }
                return false;
            case ReorgMappingStatus.Ready:
                switch (next)
                {
                    case ReorgMappingStatus.Stopped:
                        return true;
                }
                return false;
            case ReorgMappingStatus.Draining:
                switch (next)
                {
                    case ReorgMappingStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ReorgMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"reorg mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"reorg mapping"} {Code}";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Weight - (double)ExpiresAt, 2);
    }

    public ReorgMapping Copy()
    {
        return new ReorgMapping(Code, Name, Country, Title, Weight, ExpiresAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
