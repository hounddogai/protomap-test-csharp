namespace Acme.Workforce.Assets.Laptop.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a laptop draft is in its life cycle.</summary>
public enum LaptopDraftStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A laptop draft as the laptop team keeps it.</summary>
public sealed class LaptopDraft(string code, string label, double ratio, string summary, string managerId, int endsAt, string department, int quantity, int tenureMonths, LaptopDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Ratio { get; set; } = ratio;
    public string Summary { get; set; } = summary;
    public string ManagerId { get; set; } = managerId;
    public int EndsAt { get; set; } = endsAt;
    public string Department { get; set; } = department;
    public int Quantity { get; set; } = quantity;
    public int TenureMonths { get; set; } = tenureMonths;
    public LaptopDraftStatus Status { get; set; } = status;

    /// <summary>Starts a laptop draft in status requested.</summary>
    public static LaptopDraft NewLaptopDraft(string code, string label)
    {
        return new LaptopDraft(code, label, 0.0, "", "", 0, "", 0, 0, LaptopDraftStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LaptopDraftStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LaptopDraftStatus next)
    {
        switch (Status)
        {
            case LaptopDraftStatus.Requested:
                switch (next)
                {
                    case LaptopDraftStatus.Granted:
                        return true;
                }
                return false;
            case LaptopDraftStatus.Granted:
                return next == LaptopDraftStatus.Revoked || next == LaptopDraftStatus.Expired;
            case LaptopDraftStatus.Revoked:
                return next == LaptopDraftStatus.Expired;
        }
        return false;
    }

    public void MoveTo(LaptopDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"laptop draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"laptop draft"} {Code}";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Ratio * (double)TenureMonths, 2);
    }

    public LaptopDraft Copy()
    {
        return new LaptopDraft(
            Code,
            Label,
            Ratio,
            Summary,
            ManagerId,
            EndsAt,
            Department,
            Quantity,
            TenureMonths,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
