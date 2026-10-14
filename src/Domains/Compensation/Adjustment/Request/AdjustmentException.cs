namespace Acme.Workforce.Compensation.Adjustment.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adjustment exception is in its life cycle.</summary>
public enum AdjustmentExceptionStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A adjustment exception. Its code is unique within a tenant.</summary>
public sealed class AdjustmentException(string code, string title, string team, double rating, double maximum, string contactEmail, double ratio, int version, AdjustmentExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Team { get; set; } = team;
    public double Rating { get; set; } = rating;
    public double Maximum { get; set; } = maximum;
    public string ContactEmail { get; set; } = contactEmail;
    public double Ratio { get; set; } = ratio;
    public int Version { get; set; } = version;
    public AdjustmentExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a adjustment exception in status created.</summary>
    public static AdjustmentException NewAdjustmentException(string code, string title)
    {
        return new AdjustmentException(code, title, "", 0.0, 0.0, "", 0.0, 0, AdjustmentExceptionStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdjustmentExceptionStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdjustmentExceptionStatus next)
    {
        switch (Status)
        {
            case AdjustmentExceptionStatus.Created:
                switch (next)
                {
                    case AdjustmentExceptionStatus.Validated:
                    case AdjustmentExceptionStatus.Retired:
                        return true;
                }
                return false;
            case AdjustmentExceptionStatus.Validated:
                return next == AdjustmentExceptionStatus.Published || next == AdjustmentExceptionStatus.Retired;
            case AdjustmentExceptionStatus.Published:
                switch (next)
                {
                    case AdjustmentExceptionStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdjustmentExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adjustment exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{"adjustment exception"} {Code}";
    }

    public double AdjustedVolume()
    {
        return Math.Round(Ratio - Rating, 2);
    }

    public AdjustmentException Copy()
    {
        return new AdjustmentException(Code, Title, Team, Rating, Maximum, ContactEmail, Ratio, Version, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
