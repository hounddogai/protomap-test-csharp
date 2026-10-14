namespace Acme.Workforce.Assets.Warranty.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a warranty exception is in its life cycle.</summary>
public enum WarrantyExceptionStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a warranty exception and its current status.</summary>
public sealed class WarrantyException(string code, string label, double weight, int endsAt, string region, bool archived, int leaveDays, double discount, double rate, string summary, WarrantyExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Weight { get; set; } = weight;
    public int EndsAt { get; set; } = endsAt;
    public string Region { get; set; } = region;
    public bool Archived { get; set; } = archived;
    public int LeaveDays { get; set; } = leaveDays;
    public double Discount { get; set; } = discount;
    public double Rate { get; set; } = rate;
    public string Summary { get; set; } = summary;
    public WarrantyExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a warranty exception in status idle.</summary>
    public static WarrantyException NewWarrantyException(string code, string label)
    {
        return new WarrantyException(code, label, 0.0, 0, "", false, 0, 0.0, 0.0, "", WarrantyExceptionStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WarrantyExceptionStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WarrantyExceptionStatus next)
    {
        switch (Status)
        {
            case WarrantyExceptionStatus.Idle:
                switch (next)
                {
                    case WarrantyExceptionStatus.Warming:
                    case WarrantyExceptionStatus.Stopped:
                        return true;
                }
                return false;
            case WarrantyExceptionStatus.Warming:
                return next == WarrantyExceptionStatus.Stopped;
            case WarrantyExceptionStatus.Ready:
                switch (next)
                {
                    case WarrantyExceptionStatus.Draining:
                    case WarrantyExceptionStatus.Stopped:
                        return true;
                }
                return false;
            case WarrantyExceptionStatus.Draining:
                switch (next)
                {
                    case WarrantyExceptionStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WarrantyExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"warranty exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double WeightedTotal()
    {
        return Math.Round(Weight * Discount, 2);
    }

    public WarrantyException Copy()
    {
        return new WarrantyException(
            Code,
            Label,
            Weight,
            EndsAt,
            Region,
            Archived,
            LeaveDays,
            Discount,
            Rate,
            Summary,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
