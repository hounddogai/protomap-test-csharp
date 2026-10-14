namespace Acme.Workforce.Engagement.TownHall.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a town hall target is in its life cycle.</summary>
public enum TownHallTargetStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One town hall target, with what the planners need to act on it.</summary>
public sealed class TownHallTarget(string code, string title, double rate, string timezone, int updatedAt, int retries, int quantity, int graceDays, string jobCode, string channel, TownHallTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Rate { get; set; } = rate;
    public string Timezone { get; set; } = timezone;
    public int UpdatedAt { get; set; } = updatedAt;
    public int Retries { get; set; } = retries;
    public int Quantity { get; set; } = quantity;
    public int GraceDays { get; set; } = graceDays;
    public string JobCode { get; set; } = jobCode;
    public string Channel { get; set; } = channel;
    public TownHallTargetStatus Status { get; set; } = status;

    /// <summary>Starts a town hall target in status created.</summary>
    public static TownHallTarget NewTownHallTarget(string code, string title)
    {
        return new TownHallTarget(code, title, 0.0, "", 0, 0, 0, 0, "", "", TownHallTargetStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TownHallTargetStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TownHallTargetStatus next)
    {
        switch (Status)
        {
            case TownHallTargetStatus.Created:
                return next == TownHallTargetStatus.Published;
            case TownHallTargetStatus.Validated:
                switch (next)
                {
                    case TownHallTargetStatus.Published:
                        return true;
                }
                return false;
            case TownHallTargetStatus.Published:
                switch (next)
                {
                    case TownHallTargetStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TownHallTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"town hall target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)Retries + (double)GraceDays, 2);
    }

    public TownHallTarget Copy()
    {
        return new TownHallTarget(
            Code,
            Title,
            Rate,
            Timezone,
            UpdatedAt,
            Retries,
            Quantity,
            GraceDays,
            JobCode,
            Channel,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
