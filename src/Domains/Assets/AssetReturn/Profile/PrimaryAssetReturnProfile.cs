namespace Acme.Workforce.Assets.AssetReturn.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary asset return profile is in its life cycle.</summary>
public enum PrimaryAssetReturnProfileStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A primary asset return profile. Its code is unique within a tenant.</summary>
public sealed class PrimaryAssetReturnProfile(string code, string title, string category, bool visible, string locale, int leaveDays, PrimaryAssetReturnProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Category { get; set; } = category;
    public bool Visible { get; set; } = visible;
    public string Locale { get; set; } = locale;
    public int LeaveDays { get; set; } = leaveDays;
    public PrimaryAssetReturnProfileStatus Status { get; set; } = status;

    /// <summary>Starts a primary asset return profile in status requested.</summary>
    public static PrimaryAssetReturnProfile NewPrimaryAssetReturnProfile(string code, string title)
    {
        return new PrimaryAssetReturnProfile(
            code,
            title,
            "",
            false,
            "",
            0,
            PrimaryAssetReturnProfileStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryAssetReturnProfileStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryAssetReturnProfileStatus next)
    {
        switch (Status)
        {
            case PrimaryAssetReturnProfileStatus.Requested:
                switch (next)
                {
                    case PrimaryAssetReturnProfileStatus.Granted:
                    case PrimaryAssetReturnProfileStatus.Expired:
                        return true;
                }
                return false;
            case PrimaryAssetReturnProfileStatus.Granted:
                switch (next)
                {
                    case PrimaryAssetReturnProfileStatus.Revoked:
                    case PrimaryAssetReturnProfileStatus.Expired:
                        return true;
                }
                return false;
            case PrimaryAssetReturnProfileStatus.Revoked:
                switch (next)
                {
                    case PrimaryAssetReturnProfileStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryAssetReturnProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary asset return profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"primary asset return profile"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)LeaveDays;
    }

    public PrimaryAssetReturnProfile Copy()
    {
        return new PrimaryAssetReturnProfile(Code, Title, Category, Visible, Locale, LeaveDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
