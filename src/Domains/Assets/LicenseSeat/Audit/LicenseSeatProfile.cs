namespace Acme.Workforce.Assets.LicenseSeat.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a license seat profile is in its life cycle.</summary>
public enum LicenseSeatProfileStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A license seat profile. Its code is unique within a tenant.</summary>
public sealed class LicenseSeatProfile(string code, string title, string category, string region, double cost, double minimum, string locale, string label, double ratio, LicenseSeatProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Category { get; set; } = category;
    public string Region { get; set; } = region;
    public double Cost { get; set; } = cost;
    public double Minimum { get; set; } = minimum;
    public string Locale { get; set; } = locale;
    public string Label { get; set; } = label;
    public double Ratio { get; set; } = ratio;
    public LicenseSeatProfileStatus Status { get; set; } = status;

    /// <summary>Starts a license seat profile in status open.</summary>
    public static LicenseSeatProfile NewLicenseSeatProfile(string code, string title)
    {
        return new LicenseSeatProfile(code, title, "", "", 0.0, 0.0, "", "", 0.0, LicenseSeatProfileStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LicenseSeatProfileStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LicenseSeatProfileStatus next)
    {
        switch (Status)
        {
            case LicenseSeatProfileStatus.Open:
                switch (next)
                {
                    case LicenseSeatProfileStatus.Resolved:
                    case LicenseSeatProfileStatus.Reopened:
                        return true;
                }
                return false;
            case LicenseSeatProfileStatus.InReview:
                switch (next)
                {
                    case LicenseSeatProfileStatus.Archived:
                        return true;
                }
                return false;
            case LicenseSeatProfileStatus.Resolved:
                return next == LicenseSeatProfileStatus.Reopened || next == LicenseSeatProfileStatus.Archived;
            case LicenseSeatProfileStatus.Reopened:
                return next == LicenseSeatProfileStatus.Archived;
        }
        return false;
    }

    public void MoveTo(LicenseSeatProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"license seat profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Title}";
    }

    public double NetValue()
    {
        return Math.Round(Ratio + Minimum, 2);
    }

    public LicenseSeatProfile Copy()
    {
        return new LicenseSeatProfile(Code, Title, Category, Region, Cost, Minimum, Locale, Label, Ratio, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
