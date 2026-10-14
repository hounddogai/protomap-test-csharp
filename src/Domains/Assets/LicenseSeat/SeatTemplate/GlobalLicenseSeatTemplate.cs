namespace Acme.Workforce.Assets.LicenseSeat.SeatTemplate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global license seat template is in its life cycle.</summary>
public enum GlobalLicenseSeatTemplateStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a global license seat template and its current status.</summary>
public sealed class GlobalLicenseSeatTemplate(string code, string name, double rating, int capacity, string checksum, int expiresAt, GlobalLicenseSeatTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Rating { get; set; } = rating;
    public int Capacity { get; set; } = capacity;
    public string Checksum { get; set; } = checksum;
    public int ExpiresAt { get; set; } = expiresAt;
    public GlobalLicenseSeatTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a global license seat template in status proposed.</summary>
    public static GlobalLicenseSeatTemplate NewGlobalLicenseSeatTemplate(string code, string name)
    {
        return new GlobalLicenseSeatTemplate(code, name, 0.0, 0, "", 0, GlobalLicenseSeatTemplateStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalLicenseSeatTemplateStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalLicenseSeatTemplateStatus next)
    {
        switch (Status)
        {
            case GlobalLicenseSeatTemplateStatus.Proposed:
                switch (next)
                {
                    case GlobalLicenseSeatTemplateStatus.Rejected:
                    case GlobalLicenseSeatTemplateStatus.Withdrawn:
                        return true;
                }
                return false;
            case GlobalLicenseSeatTemplateStatus.Approved:
                return next == GlobalLicenseSeatTemplateStatus.Rejected;
            case GlobalLicenseSeatTemplateStatus.Rejected:
                switch (next)
                {
                    case GlobalLicenseSeatTemplateStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GlobalLicenseSeatTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global license seat template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"global license seat template"} {Code}";
    }

    public double WeightedExposure()
    {
        return Math.Round(Rating - (double)ExpiresAt, 2);
    }

    public GlobalLicenseSeatTemplate Copy()
    {
        return new GlobalLicenseSeatTemplate(Code, Name, Rating, Capacity, Checksum, ExpiresAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
