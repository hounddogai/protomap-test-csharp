namespace Acme.Workforce.Payrollops.PayRun.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional pay run window is in its life cycle.</summary>
public enum RegionalPayRunWindowStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a regional pay run window and its current status.</summary>
public sealed class RegionalPayRunWindow(string code, string label, int expiresAt, int quantity, string locale, RegionalPayRunWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int ExpiresAt { get; set; } = expiresAt;
    public int Quantity { get; set; } = quantity;
    public string Locale { get; set; } = locale;
    public RegionalPayRunWindowStatus Status { get; set; } = status;

    /// <summary>Starts a regional pay run window in status created.</summary>
    public static RegionalPayRunWindow NewRegionalPayRunWindow(string code, string label)
    {
        return new RegionalPayRunWindow(code, label, 0, 0, "", RegionalPayRunWindowStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalPayRunWindowStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalPayRunWindowStatus next)
    {
        switch (Status)
        {
            case RegionalPayRunWindowStatus.Created:
                return next == RegionalPayRunWindowStatus.Validated || next == RegionalPayRunWindowStatus.Published;
            case RegionalPayRunWindowStatus.Validated:
                return next == RegionalPayRunWindowStatus.Published || next == RegionalPayRunWindowStatus.Retired;
            case RegionalPayRunWindowStatus.Published:
                return next == RegionalPayRunWindowStatus.Retired;
        }
        return false;
    }

    public void MoveTo(RegionalPayRunWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional pay run window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"regional pay run window"} {Code}";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)ExpiresAt * (double)Quantity, 2);
    }

    public RegionalPayRunWindow Copy()
    {
        return new RegionalPayRunWindow(Code, Label, ExpiresAt, Quantity, Locale, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
