namespace Acme.Workforce.Assets.Refresh.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a refresh slot is in its life cycle.</summary>
public enum RefreshSlotStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a refresh slot and its current status.</summary>
public sealed class RefreshSlot(string code, string name, double fte, double rating, bool active, string segment, RefreshSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Fte { get; set; } = fte;
    public double Rating { get; set; } = rating;
    public bool Active { get; set; } = active;
    public string Segment { get; set; } = segment;
    public RefreshSlotStatus Status { get; set; } = status;

    /// <summary>Starts a refresh slot in status requested.</summary>
    public static RefreshSlot NewRefreshSlot(string code, string name)
    {
        return new RefreshSlot(code, name, 0.0, 0.0, false, "", RefreshSlotStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RefreshSlotStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RefreshSlotStatus next)
    {
        switch (Status)
        {
            case RefreshSlotStatus.Requested:
                return next == RefreshSlotStatus.Granted || next == RefreshSlotStatus.Expired;
            case RefreshSlotStatus.Granted:
                return next == RefreshSlotStatus.Revoked || next == RefreshSlotStatus.Expired;
            case RefreshSlotStatus.Revoked:
                switch (next)
                {
                    case RefreshSlotStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RefreshSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"refresh slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double WeightedVolume()
    {
        return Math.Round(Rating + Fte, 2);
    }

    public RefreshSlot Copy()
    {
        return new RefreshSlot(Code, Name, Fte, Rating, Active, Segment, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
