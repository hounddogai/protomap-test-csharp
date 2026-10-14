namespace Acme.Workforce.Assets.Laptop.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered laptop baseline is in its life cycle.</summary>
public enum TieredLaptopBaselineStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a tiered laptop baseline and its current status.</summary>
public sealed class TieredLaptopBaseline(string code, string title, int expiresAt, string unit, List<string> tags, double cost, string summary, TieredLaptopBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Unit { get; set; } = unit;
    public List<string> Tags { get; set; } = tags;
    public double Cost { get; set; } = cost;
    public string Summary { get; set; } = summary;
    public TieredLaptopBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a tiered laptop baseline in status idle.</summary>
    public static TieredLaptopBaseline NewTieredLaptopBaseline(string code, string title)
    {
        return new TieredLaptopBaseline(
            code,
            title,
            0,
            "",
            new List<string>(),
            0.0,
            "",
            TieredLaptopBaselineStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredLaptopBaselineStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredLaptopBaselineStatus next)
    {
        switch (Status)
        {
            case TieredLaptopBaselineStatus.Idle:
                return next == TieredLaptopBaselineStatus.Warming;
            case TieredLaptopBaselineStatus.Warming:
                return next == TieredLaptopBaselineStatus.Stopped;
            case TieredLaptopBaselineStatus.Ready:
                switch (next)
                {
                    case TieredLaptopBaselineStatus.Draining:
                        return true;
                }
                return false;
            case TieredLaptopBaselineStatus.Draining:
                switch (next)
                {
                    case TieredLaptopBaselineStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredLaptopBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered laptop baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
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

    public double WeightedExposure()
    {
        return Math.Round((double)ExpiresAt * Cost, 2);
    }

    public TieredLaptopBaseline Copy()
    {
        return new TieredLaptopBaseline(Code, Title, ExpiresAt, Unit, Tags, Cost, Summary, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
