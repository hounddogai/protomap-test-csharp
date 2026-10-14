namespace Acme.Workforce.Assets.Laptop.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a laptop agreement is in its life cycle.</summary>
public enum LaptopAgreementStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One laptop agreement, with what the planners need to act on it.</summary>
public sealed class LaptopAgreement(string code, string label, int expiresAt, double cost, string grade, string region, string reviewer, LaptopAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Cost { get; set; } = cost;
    public string Grade { get; set; } = grade;
    public string Region { get; set; } = region;
    public string Reviewer { get; set; } = reviewer;
    public LaptopAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a laptop agreement in status requested.</summary>
    public static LaptopAgreement NewLaptopAgreement(string code, string label)
    {
        return new LaptopAgreement(code, label, 0, 0.0, "", "", "", LaptopAgreementStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LaptopAgreementStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LaptopAgreementStatus next)
    {
        switch (Status)
        {
            case LaptopAgreementStatus.Requested:
                switch (next)
                {
                    case LaptopAgreementStatus.Revoked:
                    case LaptopAgreementStatus.Expired:
                        return true;
                }
                return false;
            case LaptopAgreementStatus.Granted:
                return next == LaptopAgreementStatus.Expired;
            case LaptopAgreementStatus.Revoked:
                switch (next)
                {
                    case LaptopAgreementStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LaptopAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"laptop agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double AdjustedTotal()
    {
        return Math.Round(Cost - (double)ExpiresAt, 2);
    }

    public LaptopAgreement Copy()
    {
        return new LaptopAgreement(Code, Label, ExpiresAt, Cost, Grade, Region, Reviewer, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
