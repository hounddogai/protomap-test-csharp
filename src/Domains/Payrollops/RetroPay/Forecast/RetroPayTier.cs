namespace Acme.Workforce.Payrollops.RetroPay.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a retro pay tier is in its life cycle.</summary>
public enum RetroPayTierStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a retro pay tier and its current status.</summary>
public sealed class RetroPayTier(string code, string label, double threshold, int revision, string externalRef, string phone, string unit, string region, double floorValue, RetroPayTierStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Threshold { get; set; } = threshold;
    public int Revision { get; set; } = revision;
    public string ExternalRef { get; set; } = externalRef;
    public string Phone { get; set; } = phone;
    public string Unit { get; set; } = unit;
    public string Region { get; set; } = region;
    public double FloorValue { get; set; } = floorValue;
    public RetroPayTierStatus Status { get; set; } = status;

    /// <summary>Starts a retro pay tier in status requested.</summary>
    public static RetroPayTier NewRetroPayTier(string code, string label)
    {
        return new RetroPayTier(code, label, 0.0, 0, "", "", "", "", 0.0, RetroPayTierStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RetroPayTierStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RetroPayTierStatus next)
    {
        switch (Status)
        {
            case RetroPayTierStatus.Requested:
                switch (next)
                {
                    case RetroPayTierStatus.Granted:
                        return true;
                }
                return false;
            case RetroPayTierStatus.Granted:
                switch (next)
                {
                    case RetroPayTierStatus.Revoked:
                        return true;
                }
                return false;
            case RetroPayTierStatus.Revoked:
                switch (next)
                {
                    case RetroPayTierStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RetroPayTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"retro pay tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double EffectiveTotal()
    {
        return Math.Round(FloorValue + (double)Revision, 2);
    }

    public RetroPayTier Copy()
    {
        return new RetroPayTier(
            Code,
            Label,
            Threshold,
            Revision,
            ExternalRef,
            Phone,
            Unit,
            Region,
            FloorValue,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
