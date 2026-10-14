namespace Acme.Workforce.Assets.LicenseSeat.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient license seat audit is in its life cycle.</summary>
public enum LenientLicenseSeatAuditStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A lenient license seat audit. Its code is unique within a tenant.</summary>
public sealed class LenientLicenseSeatAudit(string code, string title, int updatedAt, double budget, string source, double floorValue, string label, LenientLicenseSeatAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Budget { get; set; } = budget;
    public string Source { get; set; } = source;
    public double FloorValue { get; set; } = floorValue;
    public string Label { get; set; } = label;
    public LenientLicenseSeatAuditStatus Status { get; set; } = status;

    /// <summary>Starts a lenient license seat audit in status created.</summary>
    public static LenientLicenseSeatAudit NewLenientLicenseSeatAudit(string code, string title)
    {
        return new LenientLicenseSeatAudit(code, title, 0, 0.0, "", 0.0, "", LenientLicenseSeatAuditStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientLicenseSeatAuditStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientLicenseSeatAuditStatus next)
    {
        switch (Status)
        {
            case LenientLicenseSeatAuditStatus.Created:
                return next == LenientLicenseSeatAuditStatus.Published || next == LenientLicenseSeatAuditStatus.Retired;
            case LenientLicenseSeatAuditStatus.Validated:
                return next == LenientLicenseSeatAuditStatus.Published || next == LenientLicenseSeatAuditStatus.Retired;
            case LenientLicenseSeatAuditStatus.Published:
                switch (next)
                {
                    case LenientLicenseSeatAuditStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientLicenseSeatAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient license seat audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"lenient license seat audit"} {Code}";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)UpdatedAt - Budget, 2);
    }

    public LenientLicenseSeatAudit Copy()
    {
        return new LenientLicenseSeatAudit(Code, Title, UpdatedAt, Budget, Source, FloorValue, Label, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
