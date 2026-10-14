namespace Acme.Workforce.Orgdesign.Reorg.ReorgSummary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional reorg window is in its life cycle.</summary>
public enum RegionalReorgWindowStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A regional reorg window. Its code is unique within a tenant.</summary>
public sealed class RegionalReorgWindow(string code, string label, string category, string badge, int updatedAt, RegionalReorgWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Category { get; set; } = category;
    public string Badge { get; set; } = badge;
    public int UpdatedAt { get; set; } = updatedAt;
    public RegionalReorgWindowStatus Status { get; set; } = status;

    /// <summary>Starts a regional reorg window in status requested.</summary>
    public static RegionalReorgWindow NewRegionalReorgWindow(string code, string label)
    {
        return new RegionalReorgWindow(code, label, "", "", 0, RegionalReorgWindowStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalReorgWindowStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalReorgWindowStatus next)
    {
        switch (Status)
        {
            case RegionalReorgWindowStatus.Requested:
                return next == RegionalReorgWindowStatus.Revoked;
            case RegionalReorgWindowStatus.Granted:
                switch (next)
                {
                    case RegionalReorgWindowStatus.Revoked:
                    case RegionalReorgWindowStatus.Expired:
                        return true;
                }
                return false;
            case RegionalReorgWindowStatus.Revoked:
                return next == RegionalReorgWindowStatus.Expired;
        }
        return false;
    }

    public void MoveTo(RegionalReorgWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional reorg window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"regional reorg window"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)UpdatedAt;
    }

    public RegionalReorgWindow Copy()
    {
        return new RegionalReorgWindow(Code, Label, Category, Badge, UpdatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
