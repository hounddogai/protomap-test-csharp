namespace Acme.Workforce.Payrollops.RetroPay.PayPolicy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual retro pay policy is in its life cycle.</summary>
public enum ManualRetroPayPolicyStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A manual retro pay policy. Its code is unique within a tenant.</summary>
public sealed class ManualRetroPayPolicy(string code, string label, double maximum, int updatedAt, string title, double rating, string unit, ManualRetroPayPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Maximum { get; set; } = maximum;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Title { get; set; } = title;
    public double Rating { get; set; } = rating;
    public string Unit { get; set; } = unit;
    public ManualRetroPayPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a manual retro pay policy in status created.</summary>
    public static ManualRetroPayPolicy NewManualRetroPayPolicy(string code, string label)
    {
        return new ManualRetroPayPolicy(code, label, 0.0, 0, "", 0.0, "", ManualRetroPayPolicyStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualRetroPayPolicyStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualRetroPayPolicyStatus next)
    {
        switch (Status)
        {
            case ManualRetroPayPolicyStatus.Created:
                return next == ManualRetroPayPolicyStatus.Retired;
            case ManualRetroPayPolicyStatus.Validated:
                return next == ManualRetroPayPolicyStatus.Published || next == ManualRetroPayPolicyStatus.Retired;
            case ManualRetroPayPolicyStatus.Published:
                return next == ManualRetroPayPolicyStatus.Retired;
        }
        return false;
    }

    public void MoveTo(ManualRetroPayPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual retro pay policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double WeightedExposure()
    {
        return Math.Round(Maximum + Rating, 2);
    }

    public ManualRetroPayPolicy Copy()
    {
        return new ManualRetroPayPolicy(Code, Label, Maximum, UpdatedAt, Title, Rating, Unit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
